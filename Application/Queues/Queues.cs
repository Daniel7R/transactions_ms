namespace PaymentsMS.Application.Queues
{
    public static class Queues
    {
        //produce
        //consumed in TournamentMS
        public const string GET_TOURNAMENT_BY_ID = "tournament.by_id";
        public const string GET_TOURNAMENT_INFO = "tournament.info";
        public const string GET_MATCH_INFO = "match.info";
        //consumed in TicketMs
        public const string GET_TICKET_INFO = "ticket.info";
        public const string SELL_TICKET_PARTICIPANT = "ticket.participant.sale";
        public const string SELL_TICKET_VIEWER = "ticket.viewer.sale";
        public const string VALIDATE_USER_HAS_TICKETS_TOURNAMENT = "ticket.user.tournament";
        //consumed in NotificationsAndAlerts
        public const string SEND_EMAIL_DONATION = "donation.email";

        //add user to teams
        public const string ASSIGN_TEAM = "team.assign";

        //consume
    }
}
