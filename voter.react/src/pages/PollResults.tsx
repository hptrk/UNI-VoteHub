import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { PollResultDTO } from '../api/models/PollResultDTO';
import pollsService from '../api/client/pollsService';

const PollResults = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [results, setResults] = useState<PollResultDTO | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchPollResults = async () => {
      try {
        if (!id) return;

        setLoading(true);
        setError('');
        const data = await pollsService.getPollResults(id);
        setResults(data);
      } catch (err: unknown) {
        const errorMessage =
          err && typeof err === 'object' && 'detail' in err
            ? (err.detail as string)
            : 'Failed to load poll results';
        setError(errorMessage);
      } finally {
        setLoading(false);
      }
    };

    fetchPollResults();
  }, [id]);

  // determine color based on vote percentage
  const getColorClass = (percentage: number, isUserVote: boolean) => {
    if (isUserVote) return 'bg-primary';

    if (percentage >= 60) return 'bg-success';
    if (percentage >= 30) return 'bg-info';
    if (percentage >= 15) return 'bg-warning';
    return 'bg-danger';
  };

  if (loading) {
    return (
      <div className="container mt-5 pt-3">
        <div
          className="d-flex justify-content-center align-items-center"
          style={{ minHeight: '300px' }}
        >
          <div className="text-center">
            <div
              className="spinner-border text-primary"
              role="status"
              style={{ width: '3rem', height: '3rem' }}
            >
              <span className="visually-hidden">Loading...</span>
            </div>
            <p className="mt-3 text-muted">Loading poll results...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mt-5 pt-3">
        <div
          className="alert alert-danger d-flex align-items-center rounded-3 border-0 shadow-sm fade-in"
          role="alert"
          style={{ borderRadius: '12px' }}
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="24"
            height="24"
            fill="currentColor"
            className="bi bi-exclamation-triangle-fill flex-shrink-0 me-3"
            viewBox="0 0 16 16"
          >
            <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
          </svg>
          <div className="fw-medium">{error}</div>
        </div>
        <div className="text-center mt-4">
          <button
            className="btn btn-outline-primary"
            onClick={() => navigate(-1)}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              fill="currentColor"
              className="bi bi-arrow-left me-2"
              viewBox="0 0 16 16"
            >
              <path
                fillRule="evenodd"
                d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"
              />
            </svg>
            Go Back
          </button>
        </div>
      </div>
    );
  }

  if (!results) {
    return (
      <div className="container mt-5 pt-3">
        <div className="text-center py-5 fade-in">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="64"
            height="64"
            fill="currentColor"
            className="bi bi-question-circle text-secondary mb-3"
            viewBox="0 0 16 16"
          >
            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z" />
            <path d="M5.255 5.786a.237.237 0 0 0 .241.247h.825c.138 0 .248-.113.266-.25.09-.656.54-1.134 1.342-1.134.686 0 1.314.343 1.314 1.168 0 .635-.374.927-.965 1.371-.673.489-1.206 1.06-1.168 1.987l.003.217a.25.25 0 0 0 .25.246h.811a.25.25 0 0 0 .25-.25v-.105c0-.718.273-.927 1.01-1.486.609-.463 1.244-.977 1.244-2.056 0-1.511-1.276-2.241-2.673-2.241-1.267 0-2.655.59-2.75 2.286zm1.557 5.763c0 .533.425.927 1.01.927.609 0 1.028-.394 1.028-.927 0-.552-.42-.94-1.029-.94-.584 0-1.009.388-1.009.94z" />
          </svg>
          <h3 className="mb-3">Results Not Found</h3>
          <p className="text-muted mb-4">
            The poll results you're looking for don't exist or have been
            removed.
          </p>
          <button className="btn btn-primary" onClick={() => navigate('/')}>
            View All Polls
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mt-5 pt-3">
      <div className="row justify-content-center">
        <div className="col-lg-8">
          <div className="mb-4 d-flex justify-content-between align-items-center fade-in">
            <button
              className="btn btn-outline-secondary btn-sm px-3 d-flex align-items-center"
              onClick={() => navigate(-1)}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="16"
                height="16"
                fill="currentColor"
                className="bi bi-arrow-left me-2"
                viewBox="0 0 16 16"
              >
                <path
                  fillRule="evenodd"
                  d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"
                />
              </svg>
              Back
            </button>

            <div className="badge bg-secondary p-2">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="14"
                height="14"
                fill="currentColor"
                className="bi bi-bar-chart-fill me-1"
                viewBox="0 0 16 16"
              >
                <path d="M1 11a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1v-3zm5-4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V7zm5-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V2z" />
              </svg>
              Results
            </div>
          </div>

          <div
            className="card border-0 shadow-sm slide-in"
            style={{
              borderRadius: '16px',
              overflow: 'hidden',
            }}
          >
            <div
              className="card-header p-4"
              style={{
                background: 'var(--gradient-secondary)',
                borderBottom: 'none',
              }}
            >
              <h2
                className="mb-0 text-white"
                style={{
                  fontFamily: 'Plus Jakarta Sans, sans-serif',
                  fontWeight: 700,
                }}
              >
                {results.question}
              </h2>
            </div>

            <div className="card-body p-4">
              <div className="d-flex justify-content-between mb-4 pb-3 border-bottom">
                <div className="d-flex align-items-center">
                  <div
                    className="rounded-circle p-2 bg-light me-3 d-flex align-items-center justify-content-center"
                    style={{ width: '42px', height: '42px' }}
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="18"
                      height="18"
                      fill="var(--primary-color)"
                      className="bi bi-person"
                      viewBox="0 0 16 16"
                    >
                      <path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664z" />
                    </svg>
                  </div>
                  <div>
                    <div className="text-muted small">Created By</div>
                    <div className="fw-medium">{results.creatorUsername}</div>
                  </div>
                </div>

                <div className="d-flex align-items-center">
                  <div
                    className="rounded-circle p-2 bg-light me-3 d-flex align-items-center justify-content-center"
                    style={{ width: '42px', height: '42px' }}
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="18"
                      height="18"
                      fill="var(--primary-color)"
                      className="bi bi-people"
                      viewBox="0 0 16 16"
                    >
                      <path d="M15 14s1 0 1-1-1-4-5-4-5 3-5 4 1 1 1 1h8Zm-7.978-1A.261.261 0 0 1 7 12.996c.001-.264.167-1.03.76-1.72C8.312 10.629 9.282 10 11 10c1.717 0 2.687.63 3.24 1.276.593.69.758 1.457.76 1.72l-.008.002a.274.274 0 0 1-.014.002H7.022ZM11 7a2 2 0 1 0 0-4 2 2 0 0 0 0 4Zm3-2a3 3 0 1 1-6 0 3 3 0 0 1 6 0ZM6.936 9.28a5.88 5.88 0 0 0-1.23-.247A7.35 7.35 0 0 0 5 9c-4 0-5 3-5 4 0 .667.333 1 1 1h4.216A2.238 2.238 0 0 1 5 13c0-1.01.377-2.042 1.09-2.904.243-.294.526-.569.846-.816ZM4.92 10A5.493 5.493 0 0 0 4 13H1c0-.26.164-1.03.76-1.724.545-.636 1.492-1.256 3.16-1.275ZM1.5 5.5a3 3 0 1 1 6 0 3 3 0 0 1-6 0Zm3-2a2 2 0 1 0 0 4 2 2 0 0 0 0-4Z" />
                    </svg>
                  </div>
                  <div>
                    <div className="text-muted small">Total Votes</div>
                    <div className="fw-bold fs-4 text-primary">
                      {results.totalVotes}
                    </div>
                  </div>
                </div>
              </div>

              {results.userVoteOptionId && (
                <div
                  className="alert alert-primary d-flex align-items-center rounded-3 border-0 shadow-sm mb-4 fade-in"
                  role="alert"
                  style={{
                    background: 'rgba(99, 102, 241, 0.1)',
                  }}
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="20"
                    height="20"
                    fill="var(--primary-color)"
                    className="bi bi-check-circle-fill flex-shrink-0 me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z" />
                  </svg>
                  <div>
                    <p className="mb-0 fw-medium">
                      Your vote:{' '}
                      <span className="fw-bold">
                        {
                          results.results?.find(
                            (option) =>
                              option.optionId === results.userVoteOptionId
                          )?.optionText
                        }
                      </span>
                    </p>
                  </div>
                </div>
              )}

              <div className="results-chart mt-4">
                <h5
                  className="mb-4 fw-bold"
                  style={{ fontFamily: 'Plus Jakarta Sans, sans-serif' }}
                >
                  Vote Distribution
                </h5>
                {results.results &&
                  results.results.map((option, index) => {
                    const isUserVote =
                      option.optionId === results.userVoteOptionId;
                    const colorClass = getColorClass(
                      option.percentage,
                      isUserVote
                    );
                    const delay = 0.1 * index;

                    return (
                      <div
                        key={option.optionId}
                        className="mb-4 result-item fade-in"
                        style={{ animationDelay: `${delay}s` }}
                      >
                        <div className="d-flex justify-content-between align-items-center mb-1">
                          <div className="d-flex align-items-center">
                            {isUserVote && (
                              <span className="badge bg-primary me-2 rounded-circle">
                                <svg
                                  xmlns="http://www.w3.org/2000/svg"
                                  width="8"
                                  height="8"
                                  fill="currentColor"
                                  className="bi bi-star-fill"
                                  viewBox="0 0 16 16"
                                >
                                  <path d="M3.612 15.443c-.386.198-.824-.149-.746-.592l.83-4.73L.173 6.765c-.329-.314-.158-.888.283-.95l4.898-.696L7.538.792c.197-.39.73-.39.927 0l2.184 4.327 4.898.696c.441.062.612.636.282.95l-3.522 3.356.83 4.73c.078.443-.36.79-.746.592L8 13.187l-4.389 2.256z" />
                                </svg>
                              </span>
                            )}
                            <span className="fw-medium fs-5">
                              {option.optionText}
                            </span>
                          </div>
                          <div className="fw-bold">
                            {option.voteCount} vote
                            {option.voteCount !== 1 ? 's' : ''} (
                            {option.percentage.toFixed(1)}%)
                          </div>
                        </div>
                        <div
                          className="progress"
                          style={{
                            height: '12px',
                            borderRadius: '6px',
                          }}
                        >
                          <div
                            className={`progress-bar progress-bar-striped progress-bar-animated ${colorClass}`}
                            role="progressbar"
                            style={{
                              width: `${Math.max(option.percentage, 2)}%`,
                              transition: 'width 1s ease-in-out',
                              transitionDelay: `${delay}s`,
                              boxShadow: isUserVote
                                ? '0 0 10px rgba(99, 102, 241, 0.5)'
                                : 'none',
                            }}
                            aria-valuenow={option.percentage}
                            aria-valuemin={0}
                            aria-valuemax={100}
                          >
                            {option.percentage > 8
                              ? `${option.percentage.toFixed(1)}%`
                              : ''}
                          </div>
                        </div>
                      </div>
                    );
                  })}
              </div>

              <div className="d-flex flex-wrap justify-content-between mt-5 pt-3 border-top">
                <button
                  className="btn btn-outline-secondary mb-2"
                  onClick={() => navigate(`/polls/${id}`)}
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="16"
                    height="16"
                    fill="currentColor"
                    className="bi bi-arrow-left me-2"
                    viewBox="0 0 16 16"
                  >
                    <path
                      fillRule="evenodd"
                      d="M15 8a.5.5 0 0 0-.5-.5H2.707l3.147-3.146a.5.5 0 1 0-.708-.708l-4 4a.5.5 0 0 0 0 .708l4 4a.5.5 0 0 0 .708-.708L2.707 8.5H14.5A.5.5 0 0 0 15 8z"
                    />
                  </svg>
                  Back to Poll
                </button>

                <button
                  className="btn btn-primary mb-2"
                  onClick={() => navigate('/')}
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="16"
                    height="16"
                    fill="currentColor"
                    className="bi bi-list-check me-2"
                    viewBox="0 0 16 16"
                  >
                    <path
                      fillRule="evenodd"
                      d="M5 11.5a.5.5 0 0 1 .5-.5h9a.5.5 0 0 1 0 1h-9a.5.5 0 0 1-.5-.5zm0-4a.5.5 0 0 1 .5-.5h9a.5.5 0 0 1 0 1h-9a.5.5 0 0 1-.5-.5zm0-4a.5.5 0 0 1 .5-.5h9a.5.5 0 0 1 0 1h-9a.5.5 0 0 1-.5-.5zM3.854 2.146a.5.5 0 0 1 0 .708l-1.5 1.5a.5.5 0 0 1-.708 0l-.5-.5a.5.5 0 1 1 .708-.708L1.5 3.793l1.146-1.147a.5.5 0 0 1 .708 0zm0 4a.5.5 0 0 1 0 .708l-1.5 1.5a.5.5 0 0 1-.708 0l-.5-.5a.5.5 0 1 1 .708-.708L1.5 7.793l1.146-1.147a.5.5 0 0 1 .708 0zm0 4a.5.5 0 0 1 0 .708l-1.5 1.5a.5.5 0 0 1-.708 0l-.5-.5a.5.5 0 0 1 .708-.708l.146.147 1.146-1.147a.5.5 0 0 1 .708 0z"
                    />
                  </svg>
                  View All Polls
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PollResults;
