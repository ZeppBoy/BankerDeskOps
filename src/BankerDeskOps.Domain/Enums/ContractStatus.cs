namespace BankerDeskOps.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle status of a loan contract.
    /// </summary>
    public enum ContractStatus
    {
        /// <summary>Contract is active and in force.</summary>
        Active = 0,

        /// <summary>Contract has been fulfilled — all obligations met.</summary>
        Completed = 1,

        /// <summary>Contract was terminated before completion.</summary>
        Terminated = 2
    }
}
