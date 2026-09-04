import { FC } from 'react';
import { NavIdProps, Panel, PanelHeader, Group, Header, Div, Placeholder } from '@vkontakte/vkui';

export interface DashboardProps extends NavIdProps {}

export const Dashboard: FC<DashboardProps> = ({ id }) => {
  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Панель</div>
      </PanelHeader>
      <Group header={<Header>Добро пожаловать</Header>}>
        <Div>
          <Placeholder>Здесь будет сводка — записи, доходы и быстрые действия.</Placeholder>
        </Div>
      </Group>
    </Panel>
  );
};

export default Dashboard;
