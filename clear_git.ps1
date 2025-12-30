git checkout main
git pull
$branches = git branch --merged main | Where-Object { $_ -notmatch '\*|main' }
$branches | ForEach-Object { git branch -d $_.Trim() }
