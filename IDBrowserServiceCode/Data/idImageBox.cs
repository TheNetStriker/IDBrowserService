//------------------------------------------------------------------------------
// <auto-generated>
//     Der Code wurde von einer Vorlage generiert.
//
//     Manuelle Änderungen an dieser Datei führen möglicherweise zu unerwartetem Verhalten der Anwendung.
//     Manuelle Änderungen an dieser Datei werden überschrieben, wenn der Code neu generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IDBrowserServiceCode.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class idImageBox
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public idImageBox()
        {
            this.idUser = new HashSet<idUser>();
        }
    
        public string GUID { get; set; }
        public string ShareName { get; set; }
        public Nullable<int> idProtected { get; set; }
        public string idPassword { get; set; }
        public string BoxName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<idUser> idUser { get; set; }
    }
}