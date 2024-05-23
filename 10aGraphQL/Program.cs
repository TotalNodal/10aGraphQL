using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("LibraryDb"))
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddInMemorySubscriptions();


var app = builder.Build();


app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
});

//app.MapGet("/", () => "Hello World!");

app.Run();

public class AppDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int ReleaseYear { get; set; }
    public int AuthorId { get; set; }
    public Author Author { get; set; }

}

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Book> Books { get; set; }
}

public class ErrorMessage
{
    public string Message { get; set; }
    public int ErrorCode { get; set; }
}

public class SuccessMessage
{
    public string Message { get; set; }
}

public class Query
{
    public IQueryable<Book> GetBooks([Service] AppDbContext context) => context.Books;
    public Book GetBook(int id, [Service] AppDbContext context) => context.Books.FirstOrDefault(b => b.Id == id);
    public IQueryable<Author> GetAuthors([Service] AppDbContext context) => context.Authors;
    public Author GetAuthor(int id, [Service] AppDbContext context) => context.Authors.FirstOrDefault(a => a.Id == id);
}

public class Mutation
{

    public Book CreateBook(int authorId, string title, int releaseYear, [Service] AppDbContext context)
    {
        var book = new Book
        {
            AuthorId = authorId, 
            Title = title, 
            ReleaseYear = releaseYear
        };
        context.Books.Add(book);
        context.SaveChanges();
        return book;
    }

    public Book UpdateBook(int id, int? authorId, string title, int? releaseYear, [Service] AppDbContext context)
    {
        var book = context.Books.FirstOrDefault(b => b.Id == id);
        if (book == null) return null;
        if (authorId.HasValue) book.AuthorId = authorId.Value;
        if (!string.IsNullOrEmpty(title)) book.Title = title;
        if (releaseYear.HasValue) book.ReleaseYear = releaseYear.Value;
        context.SaveChanges();
        return book;
    }

    public SuccessMessage DeleteBook(int id, [Service] AppDbContext context)
    {
        var book = context.Books.FirstOrDefault(b => b.Id == id);
        if (book == null) return new SuccessMessage { Message = "Error - Book not found!" };
        context.Books.Remove(book);
        context.SaveChanges();
        return new SuccessMessage { Message = "Book deleted" };
    }
}

public class Subscription
{
    [Subscribe]
    [Topic]
    public Book OnBookAdded([EventMessage] Book book) => book;
}
