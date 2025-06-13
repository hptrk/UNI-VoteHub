using System.ComponentModel.DataAnnotations;
using Voter.Shared.Models;

namespace Voter.Blazor.WebAssembly.ViewModels
{
    public class CreatePollViewModel
    {
        [Required(ErrorMessage = "Question is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Question must be between 5 and 200 characters")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "At least 2 options are required")]
        [MinLength(2, ErrorMessage = "At least 2 options are required")]
        public List<string> Options { get; set; } = [string.Empty, string.Empty];

        public void AddOption()
        {
            Options.Add(string.Empty);
        }

        public void RemoveOption(int index)
        {
            if (Options.Count > 2 && index >= 0 && index < Options.Count)
            {
                Options.RemoveAt(index);
            }
        }
        public bool IsValid()
        {
            DateTime now = DateTime.Today;
            return !string.IsNullOrWhiteSpace(Question) &&
                   StartDate.Date >= now &&
                   EndDate > StartDate &&
                   (EndDate - StartDate).TotalMinutes >= 15 &&
                   Options.Count >= 2 &&
                   !Options.Any(string.IsNullOrWhiteSpace);
        }

        public CreatePollRequestDTO ToCreatePollRequestDTO()
        {
            return new CreatePollRequestDTO
            {
                Question = Question,
                StartDate = StartDate.ToString("yyyy-MM-dd"),
                EndDate = EndDate.ToString("yyyy-MM-dd"),
                Options = [.. Options.Where(o => !string.IsNullOrWhiteSpace(o))]
            };
        }
    }
}
