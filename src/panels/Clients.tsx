import { FC } from 'react';
import { NavIdProps, Panel, PanelHeader, Group, Header, Div, Placeholder } from '@vkontakte/vkui';

export interface ClientsProps extends NavIdProps {}

export const Clients: FC<ClientsProps> = ({ id }) => {
  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Клиенты</div>
      </PanelHeader>
      <Group header={<Header>Список клиентов</Header>}>
        <Div>
          <Placeholder>Пусто — сюда будут добавляться клиенты.</Placeholder>
        </Div>
      </Group>
    </Panel>
  );
};

export default Clients;
