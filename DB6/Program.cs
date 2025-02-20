using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TaskItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Details { get; set; }
    public DateTime Deadline { get; set; }
    public bool Completed { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=PerfumeStoreDB;Trusted_Connection=True;");
    }
}

public class TaskManager
{
    private readonly AppDbContext context;

    public TaskManager(AppDbContext context)
    {
        this.context = context;
    }

    public async Task AddTaskAsync(TaskItem task)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
    }

    public async Task<List<TaskItem>> GetTasksAsync()
    {
        return await context.Tasks.ToListAsync();
    }

    public async Task<TaskItem> GetTaskByIdAsync(int id)
    {
        return await context.Tasks.FindAsync(id);
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task != null)
        {
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
        }
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        using (var context = new AppDbContext())
        {
            context.Database.EnsureCreated(); 

            var taskManager = new TaskManager(context);

            
            var newTask = new TaskItem
            {
                Name = "Buy groceries",
                Details = "Milk, eggs, bread",
                Deadline = DateTime.Now.AddDays(2),
                Completed = false
            };

            await taskManager.AddTaskAsync(newTask);

           
            var tasks = await taskManager.GetTasksAsync();
            foreach (var task in tasks)
            {
                Console.WriteLine($"Task: {task.Name}, Due: {task.Deadline}, Completed: {task.Completed}");
            }
        }
    }
}
