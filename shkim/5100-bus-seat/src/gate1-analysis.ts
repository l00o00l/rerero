export type UnknownRecord = Record<string, unknown>;

export type RouteCandidate = {
  routeId: string;
  routeName: string;
  regionName: string;
  routeTypeName: string;
  startStationName: string;
  endStationName: string;
  companyName: string;
  raw: UnknownRecord;
};

export type RemainSeatQuality = {
  fieldName: string | null;
  vehicleCount: number;
  positiveCount: number;
  zeroCount: number;
  minusOneCount: number;
  unknownCount: number;
  values: Array<number | null>;
  passes: boolean;
};

const REMAIN_SEAT_FIELD_CANDIDATES = [
  "remainSeatCnt",
  "remainSeatCount",
  "remainseatcnt",
  "remainSeat",
];

export function extractItems(body: unknown, itemKeys: string[]): UnknownRecord[] {
  const found = findFirstValueByKey(body, new Set(itemKeys));

  if (Array.isArray(found)) {
    return found.filter(isRecord);
  }

  if (isRecord(found)) {
    return [found];
  }

  return [];
}

export function normalizeRouteCandidates(items: UnknownRecord[]): RouteCandidate[] {
  return items.map((item) => ({
    routeId: readString(item, ["routeId", "routeID"]),
    routeName: readString(item, ["routeName", "routeNm"]),
    regionName: readString(item, ["regionName", "regionNm"]),
    routeTypeName: readString(item, ["routeTypeName", "routeTypeNm"]),
    startStationName: readString(item, ["startStationName", "startStationNm"]),
    endStationName: readString(item, ["endStationName", "endStationNm"]),
    companyName: readString(item, ["companyName", "companyNm"]),
    raw: item,
  }));
}

export function analyzeRemainSeatQuality(items: UnknownRecord[]): RemainSeatQuality {
  const fieldName = detectRemainSeatField(items);
  const values = items.map((item) => readRemainSeatValue(item, fieldName));

  const positiveCount = values.filter((value) => typeof value === "number" && value > 0).length;
  const zeroCount = values.filter((value) => value === 0).length;
  const minusOneCount = values.filter((value) => value === -1).length;
  const unknownCount = values.filter((value) => value === null).length;

  return {
    fieldName,
    vehicleCount: items.length,
    positiveCount,
    zeroCount,
    minusOneCount,
    unknownCount,
    values,
    passes: items.length > 0 && fieldName !== null && positiveCount > 0,
  };
}

function detectRemainSeatField(items: UnknownRecord[]): string | null {
  for (const field of REMAIN_SEAT_FIELD_CANDIDATES) {
    if (items.some((item) => item[field] !== undefined && item[field] !== null)) {
      return field;
    }
  }

  return null;
}

function readRemainSeatValue(item: UnknownRecord, fieldName: string | null): number | null {
  if (fieldName === null) {
    return null;
  }

  const rawValue = item[fieldName];

  if (rawValue === null || rawValue === undefined || rawValue === "") {
    return null;
  }

  const parsed = typeof rawValue === "number" ? rawValue : Number(rawValue);

  if (!Number.isFinite(parsed)) {
    return null;
  }

  return parsed;
}

function readString(item: UnknownRecord, keys: string[]): string {
  for (const key of keys) {
    const value = item[key];
    if (value !== undefined && value !== null) {
      return String(value);
    }
  }

  return "";
}

function findFirstValueByKey(value: unknown, keys: Set<string>): unknown {
  if (!isRecord(value)) {
    return undefined;
  }

  for (const [key, child] of Object.entries(value)) {
    if (keys.has(key)) {
      return child;
    }
  }

  for (const child of Object.values(value)) {
    const found = findFirstValueByKey(child, keys);
    if (found !== undefined) {
      return found;
    }
  }

  return undefined;
}

function isRecord(value: unknown): value is UnknownRecord {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

