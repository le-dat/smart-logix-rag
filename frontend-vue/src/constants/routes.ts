export interface RouteOption {
  id: number
  name: string
  base: string
  dest: string
}

export const ROUTES: RouteOption[] = [
  { id: 1, name: "Tan Son Nhat (SGN), VN ➔ Changi (SIN), SG (24h)", base: "SGN", dest: "SIN" },
  { id: 2, name: "Noi Bai (HAN), VN ➔ Incheon (ICN), KR (48h)", base: "HAN", dest: "ICN" },
  { id: 3, name: "Taoyuan (TPE), TW ➔ Noi Bai (HAN), VN (36h)", base: "TPE", dest: "HAN" },
  { id: 4, name: "Shanghai Pudong (PVG), CN ➔ Los Angeles (LAX), US (120h)", base: "PVG", dest: "LAX" },
  { id: 5, name: "Noi Bai (HAN), VN ➔ Frankfurt (FRA), DE (96h)", base: "HAN", dest: "FRA" }
]
