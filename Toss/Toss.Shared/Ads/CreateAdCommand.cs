using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Toss.Shared.Ads
{
    public class CreateAdCommand
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        [Required(ErrorMessage ="You have to add an image file")]
        public string ImageBase64 { get; set; }


        [Required]
        public string ImageMimeType { get; set; }

        [Required]
        public string Link { get; set; }
    }
}
