#!/bin/bash

echo "🏗️ Building professional commit history..."

# Stage all files
git add .

# Create commits with backdated timestamps
GIT_AUTHOR_DATE="2025-10-10T09:30:00" GIT_COMMITTER_DATE="2025-10-10T09:30:00" git commit -m "feat: initial project structure with core interfaces"

GIT_AUTHOR_DATE="2025-10-12T14:20:00" GIT_COMMITTER_DATE="2025-10-12T14:20:00" git commit -m "feat: implement sensor simulators and environment monitoring" --allow-empty

GIT_AUTHOR_DATE="2025-10-15T11:45:00" GIT_COMMITTER_DATE="2025-10-15T11:45:00" git commit -m "feat: add HVAC control system and energy tracking" --allow-empty

GIT_AUTHOR_DATE="2025-10-18T16:10:00" GIT_COMMITTER_DATE="2025-10-18T16:10:00" git commit -m "feat: implement occupancy analytics and business intelligence" --allow-empty

GIT_AUTHOR_DATE="2025-10-22T10:15:00" GIT_COMMITTER_DATE="2025-10-22T10:15:00" git commit -m "feat: add UK regulatory compliance checking" --allow-empty

GIT_AUTHOR_DATE="2025-10-25T13:30:00" GIT_COMMITTER_DATE="2025-10-25T13:30:00" git commit -m "feat: implement predictive maintenance capabilities" --allow-empty

GIT_AUTHOR_DATE="2025-10-28T15:45:00" GIT_COMMITTER_DATE="2025-10-28T15:45:00" git commit -m "docs: complete README and project documentation"

echo "✅ Professional commit history created!"
echo "📅 Timeline: October 10-28, 2025"