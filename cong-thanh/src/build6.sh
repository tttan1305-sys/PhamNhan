set -e
cd "$(dirname "$0")"
python3 v4/build.py
python3 v5/theme.py
python3 v6/icons.py
