export const GBIS_BASE_URL = "https://apis.data.go.kr/6410000";

type QueryParams = Record<string, string | number | boolean | undefined>;

export function buildGbisUrl(path: string, params: QueryParams): URL {
  const url = new URL(`${GBIS_BASE_URL}/${path}`);

  // GBIS supports XML and JSON. Gate 1 prints JSON so fields are easy to inspect.
  url.searchParams.set("format", "json");

  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined) {
      url.searchParams.set(key, String(value));
    }
  }

  return url;
}

export async function getBusRouteList(serviceKey: string, keyword: string): Promise<unknown> {
  return fetchGbisJson(
    buildGbisUrl("busrouteservice/v2/getBusRouteListv2", {
      serviceKey,
      keyword,
    }),
  );
}

export async function getBusLocationList(serviceKey: string, routeId: string): Promise<unknown> {
  return fetchGbisJson(
    buildGbisUrl("buslocationservice/v2/getBusLocationListv2", {
      serviceKey,
      routeId,
    }),
  );
}

async function fetchGbisJson(url: URL): Promise<unknown> {
  const response = await fetch(url);

  if (!response.ok) {
    throw new Error(`GBIS request failed: ${response.status} ${response.statusText}`);
  }

  return response.json();
}

