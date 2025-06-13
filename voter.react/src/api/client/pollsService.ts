import apiClient from './apiClient';
import type { PollDTO } from '../models/PollDTO';
import type { VoteRequestDTO } from '../models/VoteRequestDTO';
import type { PollResultDTO } from '../models';

export interface PollFilter {
  questionFilter?: string;
}

export interface ClosedPollFilter extends PollFilter {
  startDate?: string;
  endDate?: string;
}

export const pollsService = {
  // get all active polls with optional filter
  getActivePolls: async (filter?: PollFilter): Promise<PollDTO[]> => {
    const params: Record<string, string> = {};

    if (filter?.questionFilter) {
      params.QuestionFilter = filter.questionFilter;
    }

    return await apiClient.get<PollDTO[]>('/polls/active', params);
  },

  // get all closed polls with optional filters
  getClosedPolls: async (filter?: ClosedPollFilter): Promise<PollDTO[]> => {
    const params: Record<string, string> = {};

    if (filter?.questionFilter) {
      params.QuestionFilter = filter.questionFilter;
    }

    if (filter?.startDate) {
      params.StartDate = filter.startDate;
    }

    if (filter?.endDate) {
      params.EndDate = filter.endDate;
    }

    return await apiClient.get<PollDTO[]>('/polls/closed', params);
  },

  // single poll by id
  getPollById: async (id: string): Promise<PollDTO> => {
    return await apiClient.get<PollDTO>(`/polls/${id}`);
  },

  // vote on a poll
  vote: async (voteRequest: VoteRequestDTO): Promise<void> => {
    await apiClient.post('/polls/vote', voteRequest);
  },

  // results for closed poll
  getPollResults: async (pollId: string): Promise<PollResultDTO> => {
    return await apiClient.get<PollResultDTO>(`/polls/${pollId}/results`);
  },
};

export default pollsService;
