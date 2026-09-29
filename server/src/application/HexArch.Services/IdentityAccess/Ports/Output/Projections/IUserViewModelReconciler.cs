namespace HexArch.Services.IdentityAccess.Ports.Output.Projections
{
    public interface IUserViewModelReconciler
    {
        /// <summary>
        /// Repairs missing, stale and orphaned projected rows in the user view model.
        /// Returns the number of rows repaired.
        /// </summary>
        Task<int> Reconcile(CancellationToken cancellationToken = default);
    }
}
