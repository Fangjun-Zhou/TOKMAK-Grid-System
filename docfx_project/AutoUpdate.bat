SET PPath=%~dp0
WinSCP /console /command "option batch continue" "option confirm off" "open sftp://uploader:an123456@anw.noip.cn:22 -hostkey="ssh-ed25519 255 Ove7aJ2FwQP++qbJNEK8CilsnU/MVXhYJ7glubchqBE="" "option transfer binary" "put %PPath%_site\* /var/www/html/doc/gridsystem/" "exit" /log=%PPath%log_file.txt
EXIT /B 0