using System;
using System.Drawing;
using System.Windows.Forms;

public class BeatBar : Control
{
    public int Beats { get; set; } = 16;
    public int LitBeats { get; set; } = 0;

    public BeatBar()
    {
        this.DoubleBuffered = true;
        this.Height = 24;
        this.Width = 200;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        int spacing = 4;
        int beatWidth = (this.Width - (Beats + 1) * spacing) / Beats;
        int beatHeight = this.Height - 8;

        for (int i = 0; i < Beats; i++)
        {
            var rect = new Rectangle(
                spacing + i * (beatWidth + spacing),
                4,
                beatWidth,
                beatHeight
            );
            e.Graphics.FillRectangle(i < LitBeats ? Brushes.LimeGreen : Brushes.DarkGray, rect);
            e.Graphics.DrawRectangle(Pens.Black, rect);
        }
    }
}