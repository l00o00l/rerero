import { getBusLocationList, getBusRouteList } from "./gbis";
import { readRequiredEnv } from "./env";
import {
  analyzeRemainSeatQuality,
  extractItems,
  normalizeRouteCandidates,
} from "./gate1-analysis";

type SelectedRoute = {
  routeId: string;
  direction: string;
};

async function main(): Promise<void> {
  const serviceKey = readRequiredEnv("GBIS_SERVICE_KEY");
  const selectedRoutes = parseSelectedRoutes(process.argv.slice(2));

  console.log("Gate 1: searching GBIS route candidates for route 5100");

  const routeResponse = await getBusRouteList(serviceKey, "5100");
  const routeItems = extractItems(routeResponse, ["busRouteList"]);
  const routeCandidates = normalizeRouteCandidates(routeItems);

  console.log("\nRoute candidates:");
  console.table(
    routeCandidates.map((candidate, index) => ({
      index,
      routeId: candidate.routeId,
      routeName: candidate.routeName,
      regionName: candidate.regionName,
      routeTypeName: candidate.routeTypeName,
      startStationName: candidate.startStationName,
      endStationName: candidate.endStationName,
      companyName: candidate.companyName,
    })),
  );

  if (selectedRoutes.length === 0) {
    console.log("\nStop: choose the two routeIds for both directions, then rerun:");
    console.log("npm run gate1 -- <routeId>:<direction> <routeId>:<direction>");
    console.log("Example: npm run gate1 -- 200000115:up 200000116:down");
    return;
  }

  console.log("\nSelected routes:");
  console.table(selectedRoutes);

  for (const selectedRoute of selectedRoutes) {
    await inspectLocationResponse(serviceKey, selectedRoute);
  }
}

async function inspectLocationResponse(
  serviceKey: string,
  selectedRoute: SelectedRoute,
): Promise<void> {
  console.log(`\nCalling getBusLocationListv2 once for ${selectedRoute.direction}`);
  console.log(`routeId=${selectedRoute.routeId}`);

  const locationResponse = await getBusLocationList(serviceKey, selectedRoute.routeId);
  const locationItems = extractItems(locationResponse, ["busLocationList"]);
  const quality = analyzeRemainSeatQuality(locationItems);

  console.log("\nRaw response:");
  console.dir(locationResponse, { depth: null });

  console.log("\nRemain seat quality:");
  console.table([
    {
      direction: selectedRoute.direction,
      routeId: selectedRoute.routeId,
      fieldName: quality.fieldName ?? "(not found)",
      vehicleCount: quality.vehicleCount,
      positiveCount: quality.positiveCount,
      zeroCount: quality.zeroCount,
      minusOneCount: quality.minusOneCount,
      unknownCount: quality.unknownCount,
      passes: quality.passes,
    },
  ]);

  if (!quality.passes) {
    console.log("\nGate 1 result: FAIL for this direction. Stop and reassess the data source.");
    return;
  }

  console.log("\nGate 1 result: PASS for this direction.");
}

function parseSelectedRoutes(args: string[]): SelectedRoute[] {
  return args.map((arg, index) => {
    const [routeId, direction] = arg.split(":");

    if (!routeId) {
      throw new Error(`Invalid route argument at index ${index}: ${arg}`);
    }

    return {
      routeId,
      direction: direction || `direction-${index + 1}`,
    };
  });
}

main().catch((error: unknown) => {
  console.error(error);
  process.exitCode = 1;
});

