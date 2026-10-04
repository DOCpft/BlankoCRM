import { FC } from 'react';
import { NavIdProps, Panel, PanelHeader, Group, Header, Div, Placeholder } from '@vkontakte/vkui';

export interface SettingsProps extends NavIdProps {}

export const Settings: FC<SettingsProps> = ({ id }) => {
  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Настройки</div>
      </PanelHeader>
      <Group header={<Header>Приложение</Header>}>
        <Div>
          <Placeholder>Общие настройки приложения и интеграции.</Placeholder>
        </Div>
      </Group>
    </Panel>
  );
};

export default Settings;
