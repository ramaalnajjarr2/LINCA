    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LINCA_v1.Models
{
    public enum RequestStatus
    {
        None=0,
        Pending=1,
        Approved=2,
        Rejected=3
    }

    public class SellerRequest
    {
        [Key]
        public int Id { get; set; }

                    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; }

        // بيانات المتجر
        [Required, StringLength(120)]
        public string StoreName { get; set; }

        [StringLength(500)]
        public string? StoreDescription { get; set; }

        public string? StoreImageUrl { get; set; } // اختياري
        public string? paymentImageUrl { get; set; } // اختياري
        public string? paymentDescription { get; set; }



        // (اختياري) شو رح يبيع
        [StringLength(500)]
        public string? WhatWillYouSell { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    
}
}
