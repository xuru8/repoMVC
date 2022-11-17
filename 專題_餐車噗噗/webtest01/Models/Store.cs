//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace webtest01.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    
    public partial class Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Store()
        {
            this.Customer_Favorite = new HashSet<Customer_Favorite>();
            this.Message_Board = new HashSet<Message_Board>();
            this.Store_Business = new HashSet<Store_Business>();
            this.Store_Products = new HashSet<Store_Products>();
        }
    
        public int StoreID { get; set; }
        public string Store_Name { get; set; }
        public string Store_Class { get; set; }
        public string Address_Area { get; set; }
        public string Address_City { get; set; }
        public string Address_Local { get; set; }
        public string Introduce { get; set; }
        public string Owner_Name { get; set; }
        public string Phone { get; set; }
        public string Email_Account { get; set; }
        public string Password { get; set; }
        public Nullable<System.DateTime> Creation_At { get; set; }
        public string Picture { get; set; }
        public string EmailIdentify { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Customer_Favorite> Customer_Favorite { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

        [JsonIgnore]
        public virtual ICollection<Message_Board> Message_Board { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Store_Business> Store_Business { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<Store_Products> Store_Products { get; set; }
    }
}
