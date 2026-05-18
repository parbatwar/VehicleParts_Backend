using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleParts.Domain.Models;

public enum PaymentStatus
{
    Paid,
    Credit,
    KhaltiPending,
    KhaltiPaid,
    Overdue
}

public class SalesInvoice
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer Customer { get; set; } = null!;
    
    [Required]
    public int StaffId { get; set; }

    [ForeignKey(nameof(StaffId))]
    public Staff Staff { get; set; } = null!;
    
    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;
    
    public string? KhaltiPidx { get; set; }
    
    public bool EmailSent { get; set; } = false;
    
    public ICollection<SalesItem> Items { get; set; } = new List<SalesItem>();
}