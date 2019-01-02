﻿namespace SpecificaThor
{
    public interface ISpecificationResults<TCandidate>
    {
        bool IsValid { get; }
        string ErrorMessage { get; }
        int TotalOfErrors { get; }

        bool HasError<TSpecification>() where TSpecification : ISpecification<TCandidate>;
    }
}
