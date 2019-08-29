namespace Accel
{
    public interface ICommand
    {
        byte[] Text();
        void ParseResponse( byte[] resptext );
    }
}
