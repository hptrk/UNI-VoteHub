export interface CreatePollRequestDTO {
  question: string;
  startDate: string;
  endDate: string;
  options: string[];
}
