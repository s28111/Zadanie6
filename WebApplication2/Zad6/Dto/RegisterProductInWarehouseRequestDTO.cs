﻿using System.ComponentModel.DataAnnotations;

namespace Zad6.Dto;

public class RegisterProductInWarehouseRequestDTO
{
    [Required]
    public int? IdProduct { get; set; }
    
    [Required]
    public int? IdWarehouse { get; set; }
    
    [Required]
    public int IdOrder { get; set; }
    
    [Required]
    public int Amount { get; set; }
    
    [Required]
    public DateTime? CreatedAt { get; set; }
}