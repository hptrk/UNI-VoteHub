import type { PollOptionDTO } from './PollOptionDTO';

export interface PollDTO {
  id: number;
  question: string;
  startDate: string;
  endDate: string;
  creatorId: string;
  creatorEmail: string;
  options: PollOptionDTO[];
  creationDate: string;
  isActive: boolean;
  userHasVoted: boolean;
}
