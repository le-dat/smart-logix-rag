namespace SmartLogix.WebApi.DTOs
{
    public record CustomerDto(
        int Id,
        string Name,
        string Email,
        string Phone
    );

    public record CustomerCreateDto(
        string Name,
        string Email,
        string Phone
    );

    public record CustomerUpdateDto(
        int Id,
        string Name,
        string Email,
        string Phone
    );
}
