export type RootStackParamList = {
  Home: undefined;
  Login: undefined;
  Register: undefined;
  SpacesList: undefined;
  CreateSpace: undefined;
  SpaceDetail: { spaceId: number };
  SpaceMemoryWall: { spaceId: number; spaceName: string };
  ScanQr: undefined;
  Join: { joinCode: string };
  UploadMemory: { joinCode: string; spaceName: string };
  MemoryWall: { joinCode: string; spaceName: string };
};
