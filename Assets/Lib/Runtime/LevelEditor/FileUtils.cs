using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UnityEngine;

public static class FileUtils
{
    public static void Save(StringBuilder contents)
    {
        SaveFileDialog fileDlg = new SaveFileDialog();
        fileDlg.Filter = "txt files (*.txt)|*.txt|All files(*.*)|*.*";
        fileDlg.FilterIndex = 2;
        fileDlg.OverwritePrompt = true;
        fileDlg.RestoreDirectory = true;

        if(fileDlg.ShowDialog() == DialogResult.OK)
        {
            Stream ioStream = fileDlg.OpenFile();
            if(ioStream != null)
            {
                StreamWriter writer = new StreamWriter(ioStream);
                writer.Write(contents.ToString());
            }
        }
    }

    public static StringBuilder Load()
    {
        OpenFileDialog fileDlg = new OpenFileDialog();
        fileDlg.Filter = "txt files (*.txt)|*.txt|All files(*.*)|*.*";
        fileDlg.FilterIndex = 2;
        fileDlg.RestoreDirectory = true;
        if(fileDlg.ShowDialog() == DialogResult.OK)
        {
            Stream ioStream = fileDlg.OpenFile();
            if(ioStream != null)
            {
                StreamReader reader = new StreamReader(ioStream);
                return new StringBuilder(reader.ReadToEnd());
            }         
        }
        return null;
    }
}
