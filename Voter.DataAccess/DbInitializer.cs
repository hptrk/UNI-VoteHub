using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voter.DataAccess.Models;

namespace Voter.DataAccess
{
    public static class DbInitializer
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            VoterDbContext dbContext = scope.ServiceProvider.GetRequiredService<VoterDbContext>();
            UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await dbContext.Database.MigrateAsync();

            // create roles if they don't exist
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                _ = await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                _ = await roleManager.CreateAsync(new IdentityRole("User"));
            }
            // create admin user if it doesn't exist
            string adminEmail = "admin@voter.com";
            User? admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                };
                _ = await userManager.CreateAsync(admin, "Admin123!");
                _ = await userManager.AddToRoleAsync(admin, "Admin");
            }

            // TEST USERS
            for (int i = 1; i <= 5; i++)
            {
                string testEmail = $"test{i}@voter.com";
                User? testUser = await userManager.FindByEmailAsync(testEmail);
                if (testUser == null)
                {
                    testUser = new User
                    {
                        UserName = $"testuser{i}",
                        Email = testEmail,
                        EmailConfirmed = true,
                    };
                    _ = await userManager.CreateAsync(testUser, "Test123!");
                    _ = await userManager.AddToRoleAsync(testUser, "User");
                }
            }
            // only add polls if none exist
            if (!await dbContext.Polls.AnyAsync())
            {
                DateTime now = DateTime.UtcNow;
                List<User> users = await dbContext.Users.ToListAsync();
                Random random = new();
                // tech poll questions and their answers
                Dictionary<string, string[]> techPolls = new()
                {
                    {
                        "What is the best backend framework?",
                        new[] { ".NET", "Spring Boot", "Django", "Express.js", "Laravel", "Ruby on Rails", "FastAPI" }
                    },
                    {
                        "Which frontend framework do you prefer?",
                        new[] { "React", "Angular", "Vue.js", "Svelte", "Blazor", "Next.js", "Nuxt.js" }
                    },
                    {
                        "What's your favorite programming language?",
                        new[] { "C#", "JavaScript", "Python", "Java", "TypeScript", "Rust", "Go", "Kotlin", "Swift", "PHP" }
                    },
                    {
                        "Which database do you use most?",
                        new[] { "SQL Server", "PostgreSQL", "MongoDB", "MySQL", "SQLite", "Oracle", "Redis", "DynamoDB", "Cassandra" }
                    },
                    {
                        "What code editor/IDE do you primarily use?",
                        new[] { "Visual Studio", "VS Code", "JetBrains Rider", "IntelliJ IDEA", "Sublime Text", "Vim", "Emacs", "Atom", "WebStorm" }
                    },
                    {
                        "Which cloud provider do you prefer?",
                        new[] { "Azure", "AWS", "Google Cloud", "Digital Ocean", "Heroku", "IBM Cloud", "Oracle Cloud", "Linode", "Vultr" }
                    },
                    {
                        "What's your preferred CI/CD tool?",
                        new[] { "GitHub Actions", "Jenkins", "GitLab CI", "Azure DevOps", "CircleCI", "Travis CI", "TeamCity", "Bamboo" }
                    },
                    {
                        "Which containerization technology do you use?",
                        new[] { "Docker", "Kubernetes", "Podman", "LXC", "containerd", "rkt", "None/Don't use containers" }
                    }
                };
                // general poll questions and their answers
                Dictionary<string, string[]> generalPolls = new()
                {
                    {
                        "Which season do you prefer?",
                        new[] { "Spring", "Summer", "Autumn", "Winter" }
                    },
                    {
                        "How do you take your coffee?",
                        new[] { "Black", "With milk", "With sugar", "With milk and sugar", "With plant-based milk", "I don't drink coffee" }
                    },
                    {
                        "What's your favorite movie genre?",
                        new[] { "Action", "Comedy", "Drama", "Sci-Fi", "Horror", "Romance", "Documentary", "Thriller", "Animation", "Fantasy" }
                    },
                    {
                        "How do you prefer to work?",
                        new[] { "From home", "From office", "Hybrid model", "Coworking space", "Digital nomad", "Different location each day" }
                    },
                    {
                        "How much time do you spend on social media daily?",
                        new[] { "Less than 1 hour", "1-2 hours", "2-4 hours", "4+ hours", "I don't use social media", "I use a screen time limiter" }
                    },
                    {
                        "What's your preferred way to learn new things?",
                        new[] { "Online courses", "Books", "Videos", "In-person classes", "Practice projects", "Mentorship", "Learning by doing", "Pair programming" }
                    },
                    {
                        "Which type of vacation do you prefer?",
                        new[] { "Beach holiday", "City break", "Mountain retreat", "Cultural exploration", "Adventure travel", "Road trip", "Staycation", "Cruise" }
                    },
                    {
                        "What's your preferred mode of transport?",
                        new[] { "Car", "Public transport", "Walking", "Cycling", "Motorcycle", "Electric scooter", "Ride-sharing services" }
                    },
                    {
                        "How do you prefer to read books?",
                        new[] { "Physical books", "E-reader", "Smartphone/tablet", "Audiobooks", "I don't read books" }
                    },
                    {
                        "What's your favorite type of cuisine?",
                        new[] { "Italian", "Mexican", "Chinese", "Japanese", "Indian", "French", "Mediterranean", "Thai", "American", "Middle Eastern" }
                    }
                };

                // create active polls
                List<string> activePollQuestions = [.. techPolls.Keys, .. generalPolls.Keys];
                ShuffleList(activePollQuestions, random);

                foreach (User? user in users.Take(3))
                {
                    // 4 active polls per user (12 total)
                    for (int i = 0; i < 4; i++)
                    {
                        string question = activePollQuestions[(users.IndexOf(user) * 4) + i];

                        Poll poll = new()
                        {
                            Question = question,
                            StartDate = now.AddHours(-i - 1),
                            EndDate = now.AddDays(i + 1),
                            CreatorId = user.Id,
                            Options = []
                        };

                        // options based on the question
                        string[] options = techPolls.ContainsKey(question) ? techPolls[question] : generalPolls[question];
                        foreach (string option in options)
                        {
                            poll.Options.Add(new PollOption
                            {
                                Text = option
                            });
                        }

                        _ = dbContext.Polls.Add(poll);
                    }
                }
                // closed polls (ended in the past)
                Dictionary<string, string[]> closedPollQuestions = new()
                {
                    {
                        "What project management tool do you prefer?",
                        new[] { "Jira", "Trello", "Asana", "GitHub Projects", "Notion", "Monday.com", "ClickUp", "Linear", "Azure DevOps" }
                    },
                    {
                        "Which UI component library do you use?",
                        new[] { "Bootstrap", "Material UI", "Tailwind CSS", "Chakra UI", "Ant Design", "Semantic UI", "Bulma", "Foundation", "Radix UI" }
                    },
                    {
                        "What's your favorite testing framework?",
                        new[] { "Jest", "xUnit", "NUnit", "Cypress", "Selenium", "Playwright", "TestCafe", "Mocha", "Jasmine", "Vitest" }
                    },
                    {
                        "How many monitors do you use?",
                        new[] { "1", "2", "3", "More than 3", "I use a single ultrawide", "I use a laptop screen only" }
                    },
                    {
                        "What's your preferred mobile platform?",
                        new[] { "iOS", "Android", "I use both equally", "I don't use smartphones", "Huawei/HarmonyOS", "Feature phone" }
                    },
                    {
                        "How do you stay up to date with tech news?",
                        new[] { "Twitter/X", "Reddit", "Tech blogs", "YouTube", "Podcasts", "Newsletters", "GitHub Trends", "Hacker News", "LinkedIn", "Discord communities" }
                    },
                    {
                        "What's your preferred note-taking application?",
                        new[] { "Notion", "Obsidian", "Evernote", "OneNote", "Google Keep", "Apple Notes", "Roam Research", "Pen and paper", "Logseq" }
                    },
                    {
                        "Which code repository hosting service do you prefer?",
                        new[] { "GitHub", "GitLab", "Bitbucket", "Azure DevOps", "AWS CodeCommit", "Self-hosted Git" }
                    }
                };

                List<string> closedPollsList = [.. closedPollQuestions.Keys];

                foreach (User? user in users.Take(2))
                {
                    // create 3 closed polls per user (6 total)
                    for (int i = 0; i < 3; i++)
                    {
                        int index = (users.IndexOf(user) * 3) + i;
                        string question = closedPollsList[index];

                        Poll poll = new()
                        {
                            Question = question,
                            StartDate = now.AddDays(-30).AddHours(-i),
                            EndDate = now.AddDays(-1).AddHours(-i),
                            CreatorId = user.Id,
                            Options = []
                        };

                        // add options based on the question
                        foreach (string option in closedPollQuestions[question])
                        {
                            poll.Options.Add(new PollOption
                            {
                                Text = option
                            });
                        }

                        _ = dbContext.Polls.Add(poll);
                    }
                }
                // future polls (not started yet)
                Dictionary<string, string[]> futurePolls = new()
                {
                    {
                        "Which version control system do you prefer?",
                        new[] { "Git", "Mercurial", "Subversion", "Perforce", "Fossil", "Bazaar", "Other" }
                    },
                    {
                        "What is most important in a job?",
                        new[] { "Salary", "Work-life balance", "Learning opportunities", "Company culture", "Remote work option", "Career growth", "Meaningful work", "Team dynamics", "Benefits package" }
                    },
                    {
                        "How often do you refactor your code?",
                        new[] { "Continuously", "After each feature", "When it becomes problematic", "Rarely", "During scheduled tech debt sprints", "When adding new features", "Never" }
                    },
                    {
                        "What's your preferred collaboration tool?",
                        new[] { "Microsoft Teams", "Slack", "Discord", "Google Meet", "Zoom", "Mattermost", "Element/Matrix", "Telegram", "WhatsApp" }
                    },
                    {
                        "Which AI tool do you use most in development?",
                        new[] { "GitHub Copilot", "ChatGPT", "Claude", "Gemini", "Perplexity", "Tabnine", "CodeWhisperer", "None/Don't use AI tools" }
                    },
                    {
                        "How do you prefer to document your code?",
                        new[] { "Inline comments", "External documentation", "Self-documenting code", "Wiki pages", "README files", "Documentation generators", "I don't document my code" }
                    },
                    {
                        "What's your preferred way of handling state in frontend apps?",
                        new[] { "Redux", "Context API", "MobX", "Zustand", "Recoil", "XState", "Jotai/Valtio", "Custom state management", "Local component state only" }
                    },
                    {
                        "How do you feel about TypeScript vs JavaScript?",
                        new[] { "TypeScript all the way", "JavaScript is fine on its own", "TypeScript for large projects only", "Both have their place", "I prefer another language entirely" }
                    }
                };

                List<string> futurePollsList = [.. futurePolls.Keys];

                foreach (User? user in users.Take(2))
                {
                    // create 2 future polls per user (4 total)
                    for (int i = 0; i < 2; i++)
                    {
                        int index = (users.IndexOf(user) * 2) + i;
                        string question = futurePollsList[index];

                        Poll poll = new()
                        {
                            Question = question,
                            StartDate = now.AddDays(1).AddHours(i),
                            EndDate = now.AddDays(7).AddHours(i),
                            CreatorId = user.Id,
                            Options = []
                        };

                        // add options based on the question
                        foreach (string option in futurePolls[question])
                        {
                            poll.Options.Add(new PollOption
                            {
                                Text = option
                            });
                        }

                        _ = dbContext.Polls.Add(poll);
                    }
                }
                _ = await dbContext.SaveChangesAsync();

                // add votes to some closed polls
                List<Poll> closedPolls = await dbContext.Polls.Where(p => p.EndDate < now).Include(p => p.Options).ToListAsync();

                foreach (Poll poll in closedPolls)
                {
                    foreach (User? user in users.Where(u => u.Id != poll.CreatorId).Take(random.Next(2, users.Count)))
                    {
                        // randomly select one option to vote on
                        PollOption selectedOption = poll.Options.ElementAt(random.Next(0, poll.Options.Count));

                        Vote vote = new()
                        {
                            PollId = poll.Id,
                            PollOptionId = selectedOption.Id,
                            UserId = user.Id,
                            VoteDate = poll.StartDate.AddMinutes(random.Next(1, (int)(poll.EndDate - poll.StartDate).TotalMinutes))
                        };

                        _ = dbContext.Votes.Add(vote);
                    }
                }

                // add votes to some active polls randomly
                List<Poll> activePolls = await dbContext.Polls.Where(p => p.StartDate <= now && p.EndDate > now).Include(p => p.Options).ToListAsync();

                foreach (Poll poll in activePolls)
                {
                    foreach (User? user in users.Where(u => u.Id != poll.CreatorId).Take(random.Next(0, Math.Min(3, users.Count))))
                    {
                        // randomly select one option to vote on
                        PollOption selectedOption = poll.Options.ElementAt(random.Next(0, poll.Options.Count));

                        Vote vote = new()
                        {
                            PollId = poll.Id,
                            PollOptionId = selectedOption.Id,
                            UserId = user.Id,
                            VoteDate = poll.StartDate.AddMinutes(random.Next(1, (int)Math.Min((now - poll.StartDate).TotalMinutes, 1000)))
                        };

                        _ = dbContext.Votes.Add(vote);
                    }
                }
                _ = await dbContext.SaveChangesAsync();
            }
        }

        // Helper method to shuffle a list
        private static void ShuffleList<T>(List<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
