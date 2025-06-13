import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import type { PollDTO } from '../api/models/PollDTO';
import pollsService from '../api/client/pollsService';
import type { PollFilter } from '../api/client/pollsService';
import { useFormik } from 'formik';

const Home = () => {
  const [polls, setPolls] = useState<PollDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // formik form for filters
  const filterForm = useFormik({
    initialValues: {
      questionFilter: '',
    },
    onSubmit: async (values) => {
      await fetchPolls(values);
    },
  });

  const fetchPolls = async (filter?: PollFilter) => {
    try {
      setError('');
      setLoading(true);
      const data = await pollsService.getActivePolls(filter);
      setPolls(data);
    } catch (err: unknown) {
      setError(
        typeof err === 'object' && err && 'detail' in err
          ? (err.detail as string)
          : 'Failed to load active polls'
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPolls();
  }, []);
  // even when loading, show filters
  const showSpinner = loading && (
    <div className="d-flex justify-content-center my-4">
      <div className="spinner-border" role="status">
        <span className="visually-hidden">Loading...</span>
      </div>
    </div>
  );
  return (
    <div className="container mt-5">
      <div className="row mb-4">
        <div className="col-lg-8 mx-auto text-center fade-in">
          <h2 className="section-heading">Active Polls</h2>
          <p className="text-muted mb-0">
            Vote on open polls or check results of polls you've already
            participated in
          </p>
        </div>
      </div>

      <div className="row justify-content-center">
        <div className="col-lg-10">
          <div className="card mb-5 shadow-sm border-0 slide-in">
            <div className="card-body p-4">
              <form onSubmit={filterForm.handleSubmit}>
                <div className="row g-3">
                  <div className="col-md-12">
                    <label
                      htmlFor="questionFilter"
                      className="form-label fw-medium"
                    >
                      Search Polls
                    </label>
                    <div className="input-group">
                      <span className="input-group-text bg-light border-end-0">
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          width="16"
                          height="16"
                          fill="currentColor"
                          className="bi bi-search"
                          viewBox="0 0 16 16"
                        >
                          <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0" />
                        </svg>
                      </span>
                      <input
                        type="text"
                        className="form-control"
                        id="questionFilter"
                        name="questionFilter"
                        value={filterForm.values.questionFilter}
                        onChange={filterForm.handleChange}
                        placeholder="Enter keywords to filter polls..."
                      />
                    </div>
                  </div>
                </div>
                <div className="d-flex justify-content-end mt-3">
                  <button
                    type="button"
                    className="btn btn-outline-secondary me-2"
                    onClick={() => {
                      filterForm.resetForm();
                      fetchPolls();
                    }}
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      className="bi bi-x-lg me-1"
                      viewBox="0 0 16 16"
                    >
                      <path d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8z" />
                    </svg>
                    Reset
                  </button>
                  <button type="submit" className="btn btn-primary">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      className="bi bi-filter me-1"
                      viewBox="0 0 16 16"
                    >
                      <path d="M6 10.5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5m-2-3a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5m-2-3a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5" />
                    </svg>
                    Apply Filter
                  </button>
                </div>
              </form>
            </div>
          </div>

          {showSpinner}

          {error ? (
            <div
              className="alert alert-danger d-flex align-items-center"
              role="alert"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="24"
                height="24"
                fill="currentColor"
                className="bi bi-exclamation-triangle-fill flex-shrink-0 me-2"
                viewBox="0 0 16 16"
              >
                <path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2" />
              </svg>
              <div>{error}</div>
            </div>
          ) : polls.length === 0 ? (
            <div className="text-center py-5 fade-in">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="48"
                height="48"
                fill="var(--gray-400)"
                className="bi bi-search mb-3"
                viewBox="0 0 16 16"
              >
                <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0" />
              </svg>
              <h5>No active polls available</h5>
              <p className="text-muted">
                No polls match your current search criteria. Try adjusting your
                filters.
              </p>
              <button
                className="btn btn-outline-primary mt-2"
                onClick={() => {
                  filterForm.resetForm();
                  fetchPolls();
                }}
              >
                Clear Filters
              </button>
            </div>
          ) : (
            <div className="row">
              {polls.map((poll, index) => (
                <div className="col-md-6 col-lg-6 mb-4" key={poll.id}>
                  <div
                    className={`card poll-card border-0 h-100 hover-rise scale-in`}
                    style={{
                      animationDelay: `${index * 0.1}s`,
                      opacity: loading ? 0 : 1,
                      transition: 'opacity 0.3s ease',
                    }}
                  >
                    <div className="card-body p-4">
                      <div className="d-flex justify-content-between align-items-center mb-3">
                        {poll.userHasVoted ? (
                          <div className="badge bg-success">
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              width="12"
                              height="12"
                              fill="currentColor"
                              className="bi bi-check-circle-fill me-1"
                              viewBox="0 0 16 16"
                            >
                              <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0m-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z" />
                            </svg>
                            VOTED
                          </div>
                        ) : (
                          <div className="badge bg-info">
                            <svg
                              xmlns="http://www.w3.org/2000/svg"
                              width="12"
                              height="12"
                              fill="currentColor"
                              className="bi bi-hourglass-split me-1"
                              viewBox="0 0 16 16"
                            >
                              <path d="M2.5 15a.5.5 0 1 1 0-1h1v-1a4.5 4.5 0 0 1 2.557-4.06c.29-.139.443-.377.443-.59v-.7c0-.213-.154-.451-.443-.59A4.5 4.5 0 0 1 3.5 3V2h-1a.5.5 0 0 1 0-1h11a.5.5 0 0 1 0 1h-1v1a4.5 4.5 0 0 1-2.557 4.06c-.29.139-.443.377-.443.59v.7c0 .213.154.451.443.59A4.5 4.5 0 0 1 12.5 13v1h1a.5.5 0 0 1 0 1zm2-13v1c0 .537.12 1.045.337 1.5h6.326c.216-.455.337-.963.337-1.5V2zm3 6.35c0 .701-.478 1.236-1.011 1.492A3.5 3.5 0 0 0 4.5 13s.866-1.299 3-1.48zm1 0v3.17c2.134.181 3 1.48 3 1.48a3.5 3.5 0 0 0-1.989-3.158C8.978 9.586 8.5 9.052 8.5 8.351z" />
                            </svg>
                            OPEN
                          </div>
                        )}
                        <small className="text-muted">
                          Ends: {new Date(poll.endDate).toLocaleDateString()}
                        </small>
                      </div>
                      <h5 className="card-title mb-3">{poll.question}</h5>
                      <div className="d-flex align-items-center">
                        {' '}
                        <div
                          className="me-2"
                          style={{
                            width: '32px',
                            height: '32px',
                            borderRadius: '50%',
                            backgroundColor: 'var(--primary-light)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            fontSize: '14px',
                            fontWeight: 'bold',
                            color: 'var(--primary-dark)',
                          }}
                        >
                          {poll.creatorEmail?.charAt(0).toUpperCase() || 'A'}
                        </div>
                        <div>
                          <small className="text-muted">Created by</small>
                          <p className="mb-0 fw-medium">
                            {poll.creatorEmail
                              ? poll.creatorEmail.split('@')[0]
                              : 'Admin'}
                          </p>
                        </div>
                      </div>
                    </div>
                    <div className="card-footer p-3">
                      {poll.userHasVoted ? (
                        <Link
                          to={`/polls/${poll.id}/results`}
                          className="btn btn-outline-secondary w-100 d-flex align-items-center justify-content-center"
                        >
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="16"
                            height="16"
                            fill="currentColor"
                            className="bi bi-bar-chart-fill me-2"
                            viewBox="0 0 16 16"
                          >
                            <path d="M1 11a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1zm5-4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1zm5-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1z" />
                          </svg>
                          View Results
                        </Link>
                      ) : (
                        <Link
                          to={`/polls/${poll.id}`}
                          className="btn btn-primary w-100 d-flex align-items-center justify-content-center"
                        >
                          <svg
                            xmlns="http://www.w3.org/2000/svg"
                            width="16"
                            height="16"
                            fill="currentColor"
                            className="bi bi-hand-index-thumb me-2"
                            viewBox="0 0 16 16"
                          >
                            <path d="M6.75 1a.75.75 0 0 1 .75.75V8a.5.5 0 0 0 1 0V5.467l.086-.004c.317-.012.637-.008.816.027.134.027.294.096.448.182.077.042.15.147.15.314V8a.5.5 0 0 0 1 0V6.435l.106-.01c.316-.024.584-.01.708.04.118.046.3.207.486.43.081.096.15.19.2.259V8.5a.5.5 0 1 0 1 0v-1h.342a1 1 0 0 1 .995 1.1l-.271 2.715a2.5 2.5 0 0 1-.317.991l-1.395 2.442a.5.5 0 0 1-.434.252H6.118a.5.5 0 0 1-.447-.276l-1.232-2.465-2.512-4.185a.517.517 0 0 1 .809-.631l2.41 2.41A.5.5 0 0 0 6 9.5V1.75A.75.75 0 0 1 6.75 1M8.5 4.466V1.75a1.75 1.75 0 1 0-3.5 0v6.543L3.443 6.736A1.517 1.517 0 0 0 1.07 8.588l2.491 4.153 1.215 2.43A1.5 1.5 0 0 0 6.118 16h6.302a1.5 1.5 0 0 0 1.302-.756l1.395-2.441a3.5 3.5 0 0 0 .444-1.389l.271-2.715a2 2 0 0 0-1.99-2.199h-.581a5 5 0 0 0-.195-.248c-.191-.229-.51-.568-.88-.716-.364-.146-.846-.132-1.158-.108l-.132.012a1.26 1.26 0 0 0-.56-.642 2.6 2.6 0 0 0-.738-.288c-.31-.062-.739-.058-1.05-.046zm2.094 2.025" />
                          </svg>
                          Vote Now
                        </Link>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Home;
