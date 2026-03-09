using Prolog.NET.Model;
using Prolog.NET.Model.Generated;

namespace Prolog.NET.Model;

public static class ConferenceExample
{
    /// <summary>
    /// A small conference scheduling database.
    ///
    /// Three named rooms each have three available time slots, and any
    /// registered speaker can be booked into a session(Speaker, Room, Slot)
    /// triple.  The generator emits:
    ///
    ///   Room     — IRoom,    Amphitheater / Boardroom / Seminar
    ///   Slot     — ISlot,    Morning / Afternoon / Evening
    ///   Speaker  — ISpeaker, Alice / Bob / Carol / Dan
    ///   Booking  — IBooking, Session&lt;TSpeaker,TRoom,TSlot&gt;
    ///                        where TSpeaker : ISpeaker
    ///                              TRoom    : IRoom
    ///                              TSlot    : ISlot
    /// </summary>
    [GenerateRelationTypes]
    public static readonly PrologDatabase Conference = new([

        // ── Rooms ────────────────────────────────────────────────────────────
        PrologFact.Of("room", PrologAtom.Of("amphitheater")),
        PrologFact.Of("room", PrologAtom.Of("boardroom")),
        PrologFact.Of("room", PrologAtom.Of("seminar")),

        // ── Time slots ───────────────────────────────────────────────────────
        PrologFact.Of("slot", PrologAtom.Of("morning")),
        PrologFact.Of("slot", PrologAtom.Of("afternoon")),
        PrologFact.Of("slot", PrologAtom.Of("evening")),

        // ── Registered speakers ──────────────────────────────────────────────
        PrologFact.Of("speaker", PrologAtom.Of("alice")),
        PrologFact.Of("speaker", PrologAtom.Of("bob")),
        PrologFact.Of("speaker", PrologAtom.Of("carol")),
        PrologFact.Of("speaker", PrologAtom.Of("dan")),

        // ── A booking is a session/3 compound term ───────────────────────────
        // booking(session(Speaker, Room, Slot)) :-
        //     speaker(Speaker), room(Room), slot(Slot).
        //
        // The generator sees Speaker/Room/Slot appear in speaker/room/slot body
        // goals and emits the cross-relation where-constraints automatically.
        PrologRule.Of("booking",
            [PrologCompoundTerm.Of("session",
                new PrologVariable(new VariableName("Speaker")),
                new PrologVariable(new VariableName("Room")),
                new PrologVariable(new VariableName("Slot")))],
            [PrologCompoundTerm.Of("speaker", new PrologVariable(new VariableName("Speaker"))),
             PrologCompoundTerm.Of("room",    new PrologVariable(new VariableName("Room"))),
             PrologCompoundTerm.Of("slot",    new PrologVariable(new VariableName("Slot")))]),
    ]);

    // ── Usage examples ────────────────────────────────────────────────────────

    // Fix Alice in the amphitheater on a morning slot.
    // All three type arguments are concrete case types — fully ground query.
    public static Booking.Query<Booking.Session<Speaker.Alice, Room.Amphitheater, Slot.Morning>>
        AliceMorningAmphitheater()
        => Booking.QuerySession(
            new QueryArgument<Booking.Session<Speaker.Alice, Room.Amphitheater, Slot.Morning>>
                .AtomArgument(new Atom<Booking.Session<Speaker.Alice, Room.Amphitheater, Slot.Morning>>(
                    new Booking.Session<Speaker.Alice, Room.Amphitheater, Slot.Morning>(
                        new Speaker.Alice(), new Room.Amphitheater(), new Slot.Morning()))));

    // Leave the room open as a variable — the type parameter TRoom is still
    // constrained to Room.IRoom so only valid room types can be substituted.
    public static Booking.Query<Booking.Session<Speaker.Bob, TRoom, Slot.Afternoon>>
        BobAfternoonAnyRoom<TRoom>(QueryArgument<Booking.Session<Speaker.Bob, TRoom, Slot.Afternoon>> arg)
        where TRoom : struct, Room.IRoom
        => Booking.QuerySession(arg);

    // Leave all three positions open for a general search query.
    public static Booking.Query<Booking.Session<TSpeaker, TRoom, TSlot>>
        AnyBooking<TSpeaker, TRoom, TSlot>(
            QueryArgument<Booking.Session<TSpeaker, TRoom, TSlot>> arg)
        where TSpeaker : struct, Speaker.ISpeaker
        where TRoom    : struct, Room.IRoom
        where TSlot    : struct, Slot.ISlot
        => Booking.QuerySession(arg);
}
