﻿using System.Collections.Generic;
using System.Linq;
using SpecificaThor.Enums;
using SpecificaThor.Extensions;
using SpecificaThor.Structure;

namespace SpecificaThor
{
    public static partial class Specification
    {
        public static IValidationSpecification<TCandidate> Create<TCandidate>(TCandidate candidate)
            => new ValidationSpecification<TCandidate>(candidate);

        internal sealed class ValidationSpecification<TCandidate> : IValidationSpecification<TCandidate>
        {
            private readonly TCandidate _candidate;

            internal ValidationSpecification(TCandidate candidate)
                => _candidate = candidate;

            public ISingleOperator<TCandidate> Is<TSpecification>() where TSpecification : ISpecification<TCandidate>, new()
                => CreateOperator<TSpecification>(Expecting.True);

            public ISingleOperator<TCandidate> IsNot<TSpecification>() where TSpecification : ISpecification<TCandidate>, new()
                => CreateOperator<TSpecification>(Expecting.False);

            private ISingleOperator<TCandidate> CreateOperator<TSpecification>(Expecting expecting) where TSpecification : ISpecification<TCandidate>, new()
            {
                var candidateOperator = new SingleOperator(_candidate);
                candidateOperator.AddToGroup<TSpecification>(expecting);
                return candidateOperator;
            }

            internal sealed class SingleOperator : ISingleOperator<TCandidate>
            {
                private readonly TCandidate _candidate;
                private readonly SpecificationResult<TCandidate> _result;
                private readonly List<ValidationGroup<TCandidate>> _validationGroups;

                internal SingleOperator(TCandidate candidate)
                {
                    _candidate = candidate;
                    _result = new SpecificationResult<TCandidate>();
                    _validationGroups = new List<ValidationGroup<TCandidate>>();
                    _validationGroups.AddGroup();
                }

                internal void AddToGroup<TSpecification>(Expecting expecting) where TSpecification : ISpecification<TCandidate>, new()
                    => _validationGroups.AddToGroup<TSpecification, TCandidate>(expecting);

                public ISingleOperator<TCandidate> OrIs<TSpecification>() where TSpecification : ISpecification<TCandidate>, new()
                {
                    _validationGroups.AddGroup();
                    _validationGroups.AddToGroup<TSpecification, TCandidate>(Expecting.True);
                    return this;
                }

                public ISingleOperator<TCandidate> AndIs<TSpecification>() where TSpecification : ISpecification<TCandidate>, new()
                {
                    _validationGroups.AddToGroup<TSpecification, TCandidate>(Expecting.True);
                    return this;
                }

                public ISingleOperator<TCandidate> OrIsNot<TSpecification>() where TSpecification : ISpecification<TCandidate>, new()
                {
                    _validationGroups.AddGroup();
                    _validationGroups.AddToGroup<TSpecification, TCandidate>(Expecting.False);
                    return this;
                }

                public ISingleOperator<TCandidate> AndIsNot<TSpecification>() where TSpecification : ISpecification<TCandidate>, new()
                {
                    _validationGroups.AddToGroup<TSpecification, TCandidate>(Expecting.False);
                    return this;
                }

                public ISingleOperator<TCandidate> UseThisErrorMessageIfFails(string errorMessage)
                {
                    _validationGroups.GetLastAddedValidator().CustomErrorMessage = errorMessage;
                    return this;
                }

                public ISpecificationResult<TCandidate> GetResult()
                {
                    foreach (ValidationGroup<TCandidate> validationGroup in _validationGroups)
                    {
                        var errors = validationGroup.GetFailures(_candidate);
                        _result.IsValid = !errors.Any();

                        if (_result.IsValid)
                            break;
                        else
                            _result.AddErrors(errors);
                    }

                    return _result;
                }
            }
        }
    }
}