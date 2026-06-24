import { describe, expect, it } from "vitest";
import {
  analyzeRemainSeatQuality,
  extractItems,
  normalizeRouteCandidates,
} from "../src/gate1-analysis";

describe("extractItems", () => {
  it("extracts item arrays from GBIS response bodies", () => {
    const response = {
      response: {
        msgBody: {
          busRouteList: [
            { routeId: "1", routeName: "5100" },
            { routeId: "2", routeName: "5100" },
          ],
        },
      },
    };

    expect(extractItems(response, ["busRouteList"])).toEqual([
      { routeId: "1", routeName: "5100" },
      { routeId: "2", routeName: "5100" },
    ]);
  });

  it("wraps a single GBIS item object as an array", () => {
    const response = {
      response: {
        msgBody: {
          busLocationList: { vehId: "v1", remainSeatCnt: 12 },
        },
      },
    };

    expect(extractItems(response, ["busLocationList"])).toEqual([
      { vehId: "v1", remainSeatCnt: 12 },
    ]);
  });
});

describe("normalizeRouteCandidates", () => {
  it("keeps route candidate fields needed for human direction selection", () => {
    const candidates = normalizeRouteCandidates([
      {
        routeId: "200000115",
        routeName: "5100",
        regionName: "수원",
        routeTypeName: "직행좌석",
        startStationName: "경희대",
        endStationName: "신논현역",
        companyName: "경기고속",
      },
    ]);

    expect(candidates).toEqual([
      {
        routeId: "200000115",
        routeName: "5100",
        regionName: "수원",
        routeTypeName: "직행좌석",
        startStationName: "경희대",
        endStationName: "신논현역",
        companyName: "경기고속",
        raw: expect.any(Object),
      },
    ]);
  });
});

describe("analyzeRemainSeatQuality", () => {
  it("passes when at least one vehicle has a positive remainSeatCnt", () => {
    const result = analyzeRemainSeatQuality([
      { vehId: "v1", remainSeatCnt: 12 },
      { vehId: "v2", remainSeatCnt: 0 },
      { vehId: "v3", remainSeatCnt: -1 },
      { vehId: "v4" },
    ]);

    expect(result).toMatchObject({
      fieldName: "remainSeatCnt",
      vehicleCount: 4,
      positiveCount: 1,
      zeroCount: 1,
      minusOneCount: 1,
      unknownCount: 1,
      passes: true,
    });
  });

  it("fails when values are only zero, minus one, null, or missing", () => {
    const result = analyzeRemainSeatQuality([
      { vehId: "v1", remainSeatCnt: 0 },
      { vehId: "v2", remainSeatCnt: -1 },
      { vehId: "v3", remainSeatCnt: null },
      { vehId: "v4" },
    ]);

    expect(result).toMatchObject({
      vehicleCount: 4,
      positiveCount: 0,
      zeroCount: 1,
      minusOneCount: 1,
      unknownCount: 2,
      passes: false,
    });
  });

  it("detects alternate remain seat field names defensively", () => {
    const result = analyzeRemainSeatQuality([
      { vehId: "v1", remainSeatCnt: undefined, remainSeatCount: 7 },
    ]);

    expect(result).toMatchObject({
      fieldName: "remainSeatCount",
      positiveCount: 1,
      passes: true,
    });
  });
});

