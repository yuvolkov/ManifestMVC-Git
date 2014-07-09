using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public enum ReqResultStatus
    {
        BadRequest,
        NotFound,
        InvalidOperation,
        AccessDenied,
        Success
    }

    public class RequestResult
    {
        public ReqResultStatus Status { get; set; }

        public RequestResult(ReqResultStatus status)
        {
            Status = status;
        }


        public static RequestResult Create(ReqResultStatus status)
        {
            return new RequestResult(status);
        }

        public static RequestResult<T1> Create<T1>(ReqResultStatus status, T1 d1)
        {
            return new RequestResult<T1>(status, d1);
        }

        public static RequestResult<T1,T2> Create<T1,T2>(ReqResultStatus status, T1 d1, T2 d2)
        {
            return new RequestResult<T1,T2>(status, d1, d2);
        }
    }

    public class RequestResult<T1> : RequestResult
    {
        public T1 Data1 { get; set; }

        public RequestResult(ReqResultStatus status)
            : base(status)
        {
        }

        public RequestResult(ReqResultStatus status, T1 d1)
            : base(status)
        {
            Data1 = d1;
        }
    }

    public class RequestResult<T1, T2> : RequestResult<T1>
    {
        public T2 Data2 { get; set; }

        public RequestResult(ReqResultStatus status)
            : base(status)
        {
        }

        public RequestResult(ReqResultStatus status, T1 d1)
            : base(status, d1)
        {
        }

        public RequestResult(ReqResultStatus status, T1 d1, T2 d2)
            : base(status, d1)
        {
            Data2 = d2;
        }
    }
}
