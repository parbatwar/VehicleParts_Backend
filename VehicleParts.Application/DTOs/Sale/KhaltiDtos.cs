using System.ComponentModel.DataAnnotations;

namespace VehicleParts.Application.DTOs.Sale;

public class KhaltiInitiateRequestDto
{
    [Required]
    public int InvoiceId { get; set; }

    public string ReturnUrl { get; set; } = "http://localhost:5173/payment-success";
}

public class KhaltiVerifyRequestDto
{
    [Required]
    public string Pidx { get; set; } = ""; 

    [Required]
    public int InvoiceId { get; set; }
}

public class KhaltiInitializeApiResponse
{
    public string payment_url { get; set; } = "";
    public string pidx { get; set; } = "";
}

public class KhaltiLookupApiResponse
{
    public string pidx { get; set; } = "";
    public string status { get; set; } = ""; 
    public string transaction_id { get; set; } = "";
    public long total_amount { get; set; }  
}