using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;


namespace DataLayer.Repositories
{
    public class UOWRepo<TRepo> : IDisposable
        where TRepo: UOWRepo<TRepo>, new()
    {
        private bool _ownsContext;

        private bool? _autocommitOnSuccess;
        internal bool AutocommitOnSuccess
        {
            get
            {
                if (_autocommitOnSuccess == null)
                    throw new InvalidOperationException("Should set Autocommit on success after initializing!");
                return _autocommitOnSuccess.Value;
            }
            set
            {
                if (_autocommitOnSuccess != null)
                    throw new InvalidOperationException("Should set Autocommit on success only after initializing!");
                _autocommitOnSuccess = value;
            }
        }

        internal ManifestDBContext _context;


        protected UOWRepo()
        {
            _context = new ManifestDBContext();
            _ownsContext = true;

            _context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }


        protected UOWRepo(DbContext context)
        {
            _context = context as ManifestDBContext;
            _ownsContext = false;
        }


        public void Commit()
        {
            _context.SaveChanges();
        }


        //protected void _CommitIfAutocommit()
        //{
        //    if (_autocommitOnSuccess.Value)
        //        Commit();
        //}


        public static TRepo UOW(bool autocommitOnSuccess = false)
        {
            var repo = new TRepo();
            repo.AutocommitOnSuccess = autocommitOnSuccess;

            return repo;
        }

        //internal static TRepo AttachUOW(ManifestDBContext context, Func<ManifestDBContext, TRepo> constructNew)
        //{
        //    return constructNew(context);
        //}


        #region IDisposable
        public void Dispose()
        {
            if (_ownsContext)
                _context.Dispose();
        }
        #endregion
    }
}
