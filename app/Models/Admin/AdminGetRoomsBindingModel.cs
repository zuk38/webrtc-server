namespace ST.Web.Models.Admin
{
    using System.ComponentModel.DataAnnotations;
    using global::ST.Models;

    public class AdminGetRoomsBindingModel
    {
        public AdminGetRoomsBindingModel()
        {
            this.StartPage = 1;
        }

        public RoomStatus? Status { get; set; }

        /// <summary>
        /// Sorting expression, e.g. 'Title', '-Title' (descending), 'Owner.Name'.
        /// </summary>
        public string SortBy { get; set; }
        
        [Range(1, 100000, ErrorMessage = "Page number should be in range [1...100000].")]
        public int? StartPage { get; set; }

        [Range(1, 1000, ErrorMessage = "Page size be in range [1...1000].")]
        public int? PageSize { get; set; }
    }
}
