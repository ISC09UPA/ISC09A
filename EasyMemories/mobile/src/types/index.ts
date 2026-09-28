export interface AuthResponse {
  token: string;
  expiresAt: string;
  displayName: string;
  email: string;
}

export interface SpaceResponse {
  id: number;
  name: string;
  description: string | null;
  joinCode: string;
  joinUrl: string;
  isActive: boolean;
  createdAt: string;
  memoryCount: number;
}

export interface SpacePublicInfo {
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface MemoryResponse {
  id: number;
  guestName: string | null;
  comment: string | null;
  photoUrl: string;
  createdAt: string;
}
