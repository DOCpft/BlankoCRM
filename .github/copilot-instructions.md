# Copilot Instructions

## Рекомендации по проекту
- Для BlankoCRM frontend соблюдать предпочтения пользователя: mobile-first, sidebar drawer вместо BottomNav с full-screen/edge swipe, workspaces с master CRM первым и client VK mini app отдельно; FSD dependency direction app→pages→features→entities→shared; VKUI/VK icons изолировать в shared/ui; server state через TanStack Query, Zustand только для клиентского состояния; React Hook Form + Zod; русские ошибки, named exports, TypeScript без any; поэтапно согласовывать и после этапа давать краткую сводку.