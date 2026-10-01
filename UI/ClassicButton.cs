using System.Drawing.Drawing2D;

namespace Win7AlarmClassic.UI;

public sealed class ClassicButton : Button
{
    public ClassicButton()
    {
        FlatStyle = FlatStyle.Standard;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        BackColor = Color.FromArgb(245, 245, 245);
        ForeColor = Color.Black;
        Cursor = Cursors.Hand;
        AutoSize = false;
        Height = 30;
        Padding = new Padding(8, 0, 8, 0);
        UseVisualStyleBackColor = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width < 2 || Height < 2) return;
        using var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 3);
        using var pen = new Pen(Focused ? Color.FromArgb(51, 153, 255) : Color.FromArgb(155, 155, 155));
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawPath(pen, path);
    }

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = radius * 2;
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }
}
