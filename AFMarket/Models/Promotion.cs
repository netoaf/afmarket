using System;
using System.Collections.Generic;

namespace AFMarket.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public int ProductId { get; set; }

    public decimal DiscountPercent { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual Product Product { get; set; } = null!;
}
