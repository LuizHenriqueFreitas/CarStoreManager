namespace Teste;

public class FileProcess
{
    public bool FileExistis(string filename)
    {
        if(string.IsNullOrEmpty(filename))
        {
            throw new ArgumentNullException();
        }
        return FileExistis(filename);
    }
}
