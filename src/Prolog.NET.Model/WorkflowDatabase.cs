using Prolog.NET.Model;
using Prolog.NET.Model.Generated;

namespace Prolog.NET.Model;

public static class WorkflowExample
{
    /// <summary>
    /// Equivalent Prolog source:
    /// <code>
    /// activity(review). activity(approve). activity(reject).
    /// activity(notify). activity(wait).
    ///
    /// condition(approved). condition(rejected). condition(expired).
    ///
    /// step(done).
    /// step(abort).
    /// step(run(Act))            :- activity(Act).
    /// step(then(A, B))          :- step(A), step(B).
    /// step(both(A, B))          :- step(A), step(B).
    /// step(when(Cond, Yes, No)) :- condition(Cond), step(Yes), step(No).
    /// </code>
    /// </summary>
    [GenerateRelationTypes]
    public static readonly PrologDatabase Workflow = new([

        // Activities
        PrologFact.Of("activity", PrologAtom.Of("review")),
        PrologFact.Of("activity", PrologAtom.Of("approve")),
        PrologFact.Of("activity", PrologAtom.Of("reject")),
        PrologFact.Of("activity", PrologAtom.Of("notify")),
        PrologFact.Of("activity", PrologAtom.Of("wait")),

        // Conditions
        PrologFact.Of("condition", PrologAtom.Of("approved")),
        PrologFact.Of("condition", PrologAtom.Of("rejected")),
        PrologFact.Of("condition", PrologAtom.Of("expired")),

        // Terminal steps (atom facts)
        PrologFact.Of("step", PrologAtom.Of("done")),
        PrologFact.Of("step", PrologAtom.Of("abort")),

        // run(Act) — single-variable compound, Act constrained to IActivity
        PrologRule.Of("step",
            [PrologCompoundTerm.Of("run", new PrologVariable(new VariableName("Act")))],
            [PrologCompoundTerm.Of("activity", new PrologVariable(new VariableName("Act")))]),

        // then(A, B) — two-variable compound, both constrained to IStep (self-ref)
        PrologRule.Of("step",
            [PrologCompoundTerm.Of("then",
                new PrologVariable(new VariableName("A")),
                new PrologVariable(new VariableName("B")))],
            [PrologCompoundTerm.Of("step", new PrologVariable(new VariableName("A"))),
             PrologCompoundTerm.Of("step", new PrologVariable(new VariableName("B")))]),

        // both(A, B) — parallel, same shape as then
        PrologRule.Of("step",
            [PrologCompoundTerm.Of("both",
                new PrologVariable(new VariableName("A")),
                new PrologVariable(new VariableName("B")))],
            [PrologCompoundTerm.Of("step", new PrologVariable(new VariableName("A"))),
             PrologCompoundTerm.Of("step", new PrologVariable(new VariableName("B")))]),

        // when(Cond, Yes, No) — three variables, heterogeneous constraints
        PrologRule.Of("step",
            [PrologCompoundTerm.Of("when",
                new PrologVariable(new VariableName("Cond")),
                new PrologVariable(new VariableName("Yes")),
                new PrologVariable(new VariableName("No")))],
            [PrologCompoundTerm.Of("condition", new PrologVariable(new VariableName("Cond"))),
             PrologCompoundTerm.Of("step",      new PrologVariable(new VariableName("Yes"))),
             PrologCompoundTerm.Of("step",      new PrologVariable(new VariableName("No")))]),
    ]);

    // ── Usage examples ────────────────────────────────────────────────────────

    // Fully ground: then(run(review), run(notify))
    // "review, then notify" — both positions are concrete activity types
    public static Step.Query<Step.Then<Step.Run<Activity.Review>, Step.Run<Activity.Notify>>>
        ReviewThenNotify()
        => Step.QueryThen(
            new QueryArgument<Step.Then<Step.Run<Activity.Review>, Step.Run<Activity.Notify>>>
                .AtomArgument(new Atom<Step.Then<Step.Run<Activity.Review>, Step.Run<Activity.Notify>>>(
                    new Step.Then<Step.Run<Activity.Review>, Step.Run<Activity.Notify>>(
                        new Step.Run<Activity.Review>(new Activity.Review()),
                        new Step.Run<Activity.Notify>(new Activity.Notify())))));

    // Deeply nested: then(run(review), when(approved, run(notify), abort))
    // "review, then if approved → notify, else → abort"
    public static Step.Query<Step.Then<Step.Run<Activity.Review>,
                                       Step.When<Condition.Approved,
                                                 Step.Run<Activity.Notify>,
                                                 Step.Abort>>>
        ReviewThenBranch()
        => Step.QueryThen(
            new QueryArgument<Step.Then<Step.Run<Activity.Review>,
                                        Step.When<Condition.Approved,
                                                  Step.Run<Activity.Notify>,
                                                  Step.Abort>>>
                .AtomArgument(new Atom<Step.Then<Step.Run<Activity.Review>,
                                                  Step.When<Condition.Approved,
                                                            Step.Run<Activity.Notify>,
                                                            Step.Abort>>>(
                    new Step.Then<Step.Run<Activity.Review>,
                                  Step.When<Condition.Approved,
                                            Step.Run<Activity.Notify>,
                                            Step.Abort>>(
                        new Step.Run<Activity.Review>(new Activity.Review()),
                        new Step.When<Condition.Approved, Step.Run<Activity.Notify>, Step.Abort>(
                            new Condition.Approved(),
                            new Step.Run<Activity.Notify>(new Activity.Notify()),
                            new Step.Abort())))));

    // Open-ended: leave the condition type as a generic parameter
    // caller can supply any ICondition without changing the method
    public static Step.Query<Step.When<TCond, Step.Done, Step.Abort>>
        AnyConditionOrAbort<TCond>(QueryArgument<Step.When<TCond, Step.Done, Step.Abort>> arg)
        where TCond : struct, Condition.ICondition
        => Step.QueryWhen(arg);

    // ── Any<T> / variable-position examples ──────────────────────────────────────

    // step(run(Act)) where Act is a free variable.
    // Activity.Any<T> satisfies IActivity (via its interface), so Step.Run<Activity.Any<T>>
    // satisfies IStep. T is unconstrained — it's a phantom type tracking the variable.
    public static Step.Query<Step.Run<Activity.Any<T>>>
        RunAnyActivity<T>(QueryArgument<Step.Run<Activity.Any<T>>> arg)
        => Step.QueryRun(arg);

    // Compose: activity(Act), step(run(Act)) — shared logical variable Act.
    // The shared type parameter T links the two query positions the way a Prolog variable
    // links two goals in a clause body.
    public static (Activity.Query<Activity.Any<T>>, Step.Query<Step.Run<Activity.Any<T>>>)
        AllRunSteps<T>(
            QueryArgument<Activity.Any<T>> actArg,
            QueryArgument<Step.Run<Activity.Any<T>>> stepArg)
        => (Activity.QueryAny(actArg), Step.QueryRun(stepArg));

    // ── Type-safety rejections (compile-time errors, left as comments) ────────
    //
    //   Step.Run<Condition.Approved>          — Approved is ICondition, not IActivity ✗
    //   Step.When<Activity.Review, ...>       — Review is IActivity, not ICondition ✗
    //   Step.Then<Activity.Review, Step.Done> — Review is IActivity, not IStep ✗
}
