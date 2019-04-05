using System;
using System.Collections.Generic;
using System.Text;

namespace Decos.Security.Auditing.EntityFrameworkCore
{
    public interface IHasParentContext
    {
        IAuditContext ParentContext { get; set; }
    }
}
