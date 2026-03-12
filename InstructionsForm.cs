using System.Drawing;

namespace OWTrackerDesktop;

public class InstructionsForm : Form
{
    public InstructionsForm()
    {
        Text = "How to get queue notifications on your phone";
        Size = new Size(480, 420);
        MinimumSize = new Size(400, 350);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var text = @"1. Keep this app running
   Overwatch Queue Tracker must be open on your PC.

2. Your phone and PC need to be on the same WiFi network (or same internet connection).

3. In this window, look for the line that says Server: followed by the IP address. You will enter this into the app on your phone.

4. Open the **Overwatch Personal Tracker** app on your phone
   Select the 'Desktop' tab.

5. Enter the IP address
   Type the numbers before the colon (e.g. 192.168.1.105). You can ignore the :8080 part. 
   Tap 'Connect'.

6. You're done
   When a game is found, your phone will show a notification.

If it doesn't connect:
• Make sure this app is running on your PC.
• Double-check the IP address.
• Confirm your phone and PC are on the same WiFi.";

        var textBox = new TextBox
        {
            Text = text,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.None,
            BackColor = SystemColors.Window,
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(20, 20),
            Size = new Size(420, 310),
            WordWrap = true,
            TabStop = false
        };

        var okButton = new Button
        {
            Text = "OK",
            Size = new Size(100, 32),
            Location = new Point(360, 340),
            DialogResult = DialogResult.OK
        };
        okButton.Click += (_, _) => Close();

        Controls.Add(textBox);
        Controls.Add(okButton);
        AcceptButton = okButton;
        CancelButton = okButton;
    }
}
