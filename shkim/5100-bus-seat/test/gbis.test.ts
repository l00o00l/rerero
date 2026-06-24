import { describe, expect, it } from "vitest";
import { buildGbisUrl } from "../src/gbis";

describe("buildGbisUrl", () => {
  it("builds route list v2 URLs with keyword and json format", () => {
    const url = buildGbisUrl("busrouteservice/v2/getBusRouteListv2", {
      serviceKey: "KEY",
      keyword: "5100",
    });

    expect(url.toString()).toBe(
      "https://apis.data.go.kr/6410000/busrouteservice/v2/getBusRouteListv2?format=json&serviceKey=KEY&keyword=5100",
    );
  });

  it("builds bus location v2 URLs with routeId and json format", () => {
    const url = buildGbisUrl("buslocationservice/v2/getBusLocationListv2", {
      serviceKey: "KEY",
      routeId: "200000115",
    });

    expect(url.toString()).toBe(
      "https://apis.data.go.kr/6410000/buslocationservice/v2/getBusLocationListv2?format=json&serviceKey=KEY&routeId=200000115",
    );
  });
});

