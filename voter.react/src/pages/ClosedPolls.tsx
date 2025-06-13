import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import type { PollDTO } from '../api/models/PollDTO';
import pollsService from '../api/client/pollsService';
import type { ClosedPollFilter } from '../api/client/pollsService';
import { useFormik } from 'formik';

const ClosedPolls = () => {
  const [polls, setPolls] = useState<PollDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  // formik form for filters
  const filterForm = useFormik({
    initialValues: {
      questionFilter: '',
      startDate: '',
      endDate: '',
    },
    onSubmit: async (values) => {
      await fetchPolls(values);
    },
  });

  const fetchPolls = async (filters?: ClosedPollFilter) => {
    try {
      setError('');
      setLoading(true);
      const data = await pollsService.getClosedPolls(filters);
      setPolls(data);
    } catch (err: unknown) {
      setError(
        typeof err === 'object' && err && 'detail' in err
          ? (err.detail as string)
          : 'Failed to load closed polls'
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPolls();
  }, []);

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
            <p className="mt-3 text-muted">Loading closed polls...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mt-5 pt-3">
      <div className="d-flex justify-content-between align-items-center mb-4 fade-in">
        <div>
          <h2
            className="mb-1"
            style={{
              fontFamily: 'Plus Jakarta Sans, sans-serif',
              fontWeight: 800,
            }}
          >
            Closed Polls
          </h2>
          <p className="text-muted">View results from polls that have ended</p>
        </div>
        <button
          className="btn btn-outline-primary d-flex align-items-center"
          onClick={() => setIsFilterOpen(!isFilterOpen)}
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="16"
            height="16"
            fill="currentColor"
            className="bi bi-funnel me-2"
            viewBox="0 0 16 16"
          >
            <path d="M1.5 1.5A.5.5 0 0 1 2 1h12a.5.5 0 0 1 .5.5v2a.5.5 0 0 1-.128.334L10 8.692V13.5a.5.5 0 0 1-.342.474l-3 1A.5.5 0 0 1 6 14.5V8.692L1.628 3.834A.5.5 0 0 1 1.5 3.5v-2zm1 .5v1.308l4.372 4.858A.5.5 0 0 1 7 8.5v5.306l2-.666V8.5a.5.5 0 0 1 .128-.334L13.5 3.308V2h-11z" />
          </svg>
          {isFilterOpen ? 'Hide Filters' : 'Show Filters'}
        </button>
      </div>

      {isFilterOpen && (
        <div
          className="card border-0 shadow-sm mb-4 slide-in"
          style={{ borderRadius: '16px', overflow: 'hidden' }}
        >
          <div className="card-body p-4">
            <form onSubmit={filterForm.handleSubmit}>
              <div className="row g-3">
                <div className="col-md-4">
                  <label
                    htmlFor="questionFilter"
                    className="form-label fw-medium mb-2"
                  >
                    Question Contains
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
                        <path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z" />
                      </svg>
                    </span>
                    <input
                      type="text"
                      className="form-control form-control-lg"
                      id="questionFilter"
                      name="questionFilter"
                      value={filterForm.values.questionFilter}
                      onChange={filterForm.handleChange}
                      placeholder="Search by keyword..."
                    />
                  </div>
                </div>
                <div className="col-md-4">
                  <label
                    htmlFor="startDate"
                    className="form-label fw-medium mb-2"
                  >
                    Start Date
                  </label>
                  <div className="input-group">
                    <span className="input-group-text bg-light border-end-0">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-calendar-plus"
                        viewBox="0 0 16 16"
                      >
                        <path d="M8 7a.5.5 0 0 1 .5.5V9H10a.5.5 0 0 1 0 1H8.5v1.5a.5.5 0 0 1-1 0V10H6a.5.5 0 0 1 0-1h1.5V7.5A.5.5 0 0 1 8 7z" />
                        <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z" />
                      </svg>
                    </span>
                    <input
                      type="date"
                      className="form-control form-control-lg"
                      id="startDate"
                      name="startDate"
                      value={filterForm.values.startDate}
                      onChange={filterForm.handleChange}
                    />
                  </div>
                </div>
                <div className="col-md-4">
                  <label
                    htmlFor="endDate"
                    className="form-label fw-medium mb-2"
                  >
                    End Date
                  </label>
                  <div className="input-group">
                    <span className="input-group-text bg-light border-end-0">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="16"
                        height="16"
                        fill="currentColor"
                        className="bi bi-calendar-x"
                        viewBox="0 0 16 16"
                      >
                        <path d="M6.146 7.146a.5.5 0 0 1 .708 0L8 8.293l1.146-1.147a.5.5 0 1 1 .708.708L8.707 9l1.147 1.146a.5.5 0 0 1-.708.708L8 9.707l-1.146 1.147a.5.5 0 0 1-.708-.708L7.293 9 6.146 7.854a.5.5 0 0 1 0-.708z" />
                        <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z" />
                      </svg>
                    </span>
                    <input
                      type="date"
                      className="form-control form-control-lg"
                      id="endDate"
                      name="endDate"
                      value={filterForm.values.endDate}
                      onChange={filterForm.handleChange}
                    />
                  </div>
                </div>
              </div>
              <div className="d-flex justify-content-end mt-4">
                <button
                  type="button"
                  className="btn btn-outline-secondary me-2 px-4 py-2"
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
                    className="bi bi-x-circle me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z" />
                    <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z" />
                  </svg>
                  Reset
                </button>
                <button type="submit" className="btn btn-primary px-4 py-2">
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    width="16"
                    height="16"
                    fill="currentColor"
                    className="bi bi-filter me-2"
                    viewBox="0 0 16 16"
                  >
                    <path d="M6 10.5a.5.5 0 0 1 .5-.5h3a.5.5 0 0 1 0 1h-3a.5.5 0 0 1-.5-.5zm-2-3a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5zm-2-3a.5.5 0 0 1 .5-.5h11a.5.5 0 0 1 0 1h-11a.5.5 0 0 1-.5-.5z" />
                  </svg>
                  Apply Filters
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {error ? (
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
      ) : polls.length === 0 ? (
        <div className="text-center py-5 my-3 fade-in">
          <div
            className="d-inline-block p-3 rounded-circle mb-4"
            style={{ background: 'rgba(108, 117, 125, 0.1)' }}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="56"
              height="56"
              fill="#6c757d"
              className="bi bi-inbox"
              viewBox="0 0 16 16"
            >
              <path d="M4.98 4a.5.5 0 0 0-.39.188L1.54 8H6a.5.5 0 0 1 .5.5 1.5 1.5 0 1 0 3 0A.5.5 0 0 1 10 8h4.46l-3.05-3.812A.5.5 0 0 0 11.02 4H4.98zm-1.17-.437A1.5 1.5 0 0 1 4.98 3h6.04a1.5 1.5 0 0 1 1.17.563l3.7 4.625a.5.5 0 0 1 .106.374l-.39 3.124A1.5 1.5 0 0 1 14.117 13H1.883a1.5 1.5 0 0 1-1.489-1.314l-.39-3.124a.5.5 0 0 1 .106-.374l3.7-4.625z" />
            </svg>
          </div>
          <h3
            className="mb-3"
            style={{
              fontFamily: 'Plus Jakarta Sans, sans-serif',
              fontWeight: 700,
            }}
          >
            No Closed Polls Found
          </h3>
          <p className="text-muted mb-4 w-75 mx-auto">
            There are no closed polls that match your current filters. Try
            adjusting your search criteria or check back later.
          </p>
          <button
            className="btn btn-primary px-4 py-2"
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
              className="bi bi-arrow-repeat me-2"
              viewBox="0 0 16 16"
            >
              <path d="M11.534 7h3.932a.25.25 0 0 1 .192.41l-1.966 2.36a.25.25 0 0 1-.384 0l-1.966-2.36a.25.25 0 0 1 .192-.41zm-11 2h3.932a.25.25 0 0 0 .192-.41L2.692 6.23a.25.25 0 0 0-.384 0L.342 8.59A.25.25 0 0 0 .534 9z" />
              <path
                fillRule="evenodd"
                d="M8 3c-1.552 0-2.94.707-3.857 1.818a.5.5 0 1 1-.771-.636A6.002 6.002 0 0 1 13.917 7H12.9A5.002 5.002 0 0 0 8 3zM3.1 9a5.002 5.002 0 0 0 8.757 2.182.5.5 0 1 1 .771.636A6.002 6.002 0 0 1 2.083 9H3.1z"
              />
            </svg>
            Reset Filters
          </button>
        </div>
      ) : (
        <div className="row g-4">
          {polls.map((poll, index) => (
            <div className="col-md-6 col-lg-4 mb-4" key={poll.id}>
              <div
                className="card h-100 border-0 shadow-sm hover-shadow slide-in"
                style={{
                  borderRadius: '16px',
                  animationDelay: `${0.05 * index}s`,
                  transition: 'transform 0.2s ease, box-shadow 0.2s ease',
                }}
              >
                <div className="card-body p-4">
                  <div className="d-flex align-items-center mb-3">
                    <span className="badge bg-danger me-2 d-flex align-items-center p-2">
                      <svg
                        xmlns="http://www.w3.org/2000/svg"
                        width="12"
                        height="12"
                        fill="currentColor"
                        className="bi bi-lock-fill me-1"
                        viewBox="0 0 16 16"
                      >
                        <path d="M8 1a2 2 0 0 1 2 2v4H6V3a2 2 0 0 1 2-2zm3 6V3a3 3 0 0 0-6 0v4a2 2 0 0 0-2 2v5a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z" />
                      </svg>
                      Closed
                    </span>
                    {poll.userHasVoted && (
                      <span className="badge bg-primary d-flex align-items-center p-2">
                        <svg
                          xmlns="http://www.w3.org/2000/svg"
                          width="12"
                          height="12"
                          fill="currentColor"
                          className="bi bi-check2-all me-1"
                          viewBox="0 0 16 16"
                        >
                          <path d="M12.354 4.354a.5.5 0 0 0-.708-.708L5 10.293 1.854 7.146a.5.5 0 1 0-.708.708l3.5 3.5a.5.5 0 0 0 .708 0l7-7zm-4.208 7-.896-.897.707-.707.543.543 6.646-6.647a.5.5 0 0 1 .708.708l-7 7a.5.5 0 0 1-.708 0z" />
                          <path d="m5.354 7.146.896.897-.707.707-.897-.896a.5.5 0 1 1 .708-.708z" />
                        </svg>
                        Voted
                      </span>
                    )}
                  </div>
                  <h5
                    className="card-title mb-3 fw-bold"
                    style={{ fontFamily: 'Plus Jakarta Sans, sans-serif' }}
                  >
                    {poll.question}
                  </h5>
                  <div className="d-flex align-items-center mb-3">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="var(--primary-color)"
                      className="bi bi-calendar-x me-2"
                      viewBox="0 0 16 16"
                    >
                      <path d="M6.146 7.146a.5.5 0 0 1 .708 0L8 8.293l1.146-1.147a.5.5 0 1 1 .708.708L8.707 9l1.147 1.146a.5.5 0 0 1-.708.708L8 9.707l-1.146 1.147a.5.5 0 0 1-.708-.708L7.293 9 6.146 7.854a.5.5 0 0 1 0-.708z" />
                    </svg>
                    <span className="text-muted">
                      Ended on{' '}
                      {new Date(poll.endDate).toLocaleDateString(undefined, {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </span>
                  </div>
                  <div className="d-flex align-items-center mb-3">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="var(--primary-color)"
                      className="bi bi-person me-2"
                      viewBox="0 0 16 16"
                    >
                      <path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H3s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10c-2.29 0-3.516.68-4.168 1.332-.678.678-.83 1.418-.832 1.664z" />
                    </svg>
                    <span className="text-muted">Created by Admin</span>
                  </div>
                </div>
                <div className="card-footer bg-white p-3 border-top-0">
                  <Link
                    to={`/polls/${poll.id}/results`}
                    className="btn btn-secondary w-100 py-2"
                    style={{
                      background: 'var(--gradient-secondary)',
                      border: 'none',
                      transition: 'all 0.3s ease',
                    }}
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      width="16"
                      height="16"
                      fill="currentColor"
                      className="bi bi-bar-chart-fill me-2"
                      viewBox="0 0 16 16"
                    >
                      <path d="M1 11a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v3a1 1 0 0 1-1 1H2a1 1 0 0 1-1-1v-3zm5-4a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v7a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V7zm5-5a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1v12a1 1 0 0 1-1 1h-2a1 1 0 0 1-1-1V2z" />
                    </svg>
                    View Results
                  </Link>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ClosedPolls;
