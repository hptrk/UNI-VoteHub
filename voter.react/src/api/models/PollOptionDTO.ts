export interface PollOptionDTO {
  id: number;
  pollId: number;
  text: string;
  userVoted?: boolean;
}
