import type { PollOptionResultDTO } from './PollOptionResultDTO';

export interface PollResultDTO {
  pollId: number;
  question: string;
  startDate: string;
  endDate: string;
  creatorUsername: string;
  creatorEmail: string;
  results: PollOptionResultDTO[];
  totalVotes: number;
  userVoteOptionId?: number;
}
