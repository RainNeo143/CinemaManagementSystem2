using System;

namespace CinemaManagementSystem.Models
{
    /// <summary>
    /// Модель пользователя системы
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Проверка, является ли пользователь администратором
        /// </summary>
        public bool IsAdmin => Role == "Администратор";

        /// <summary>
        /// Проверка, является ли пользователь обычным пользователем
        /// </summary>
        public bool IsUser => Role == "Пользователь";
    }

    /// <summary>
    /// Модель фильма
    /// </summary>
    public class Film
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int Duration { get; set; }
        public string Producer { get; set; }
        public string Country { get; set; }
        public string Actors { get; set; }
        public string AgeRating { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Модель сеанса
    /// </summary>
    public class Session
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public string FilmTitle { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public int HallNumber { get; set; }
        public string HallName { get; set; }
        public int Occupancy { get; set; }
    }

    /// <summary>
    /// Модель бронирования
    /// </summary>
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SessionId { get; set; }
        public int Row { get; set; }
        public int SeatNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }

        // Дополнительная информация для отображения
        public string FilmTitle { get; set; }
        public DateTime SessionDate { get; set; }
        public TimeSpan SessionTime { get; set; }
        public int HallNumber { get; set; }
    }

    /// <summary>
    /// Модель места в зале
    /// </summary>
    public class Seat
    {
        public int Id { get; set; }
        public int HallNumber { get; set; }
        public int Row { get; set; }
        public int SeatNumber { get; set; }
        public string SeatType { get; set; }
        public bool IsOccupied { get; set; }
    }
}