import type { UserDTO } from './UserDTO';

export interface LoginResponseDTO {
  success: boolean;
  token?: string;
  message?: string;
  user?: UserDTO;
}
