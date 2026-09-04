import { FC } from 'react';
import { NavIdProps, Panel, PanelHeader, Group, Header, Div, Placeholder } from '@vkontakte/vkui';

export interface AppointmentsProps extends NavIdProps {}

export const Appointments: FC<AppointmentsProps> = ({ id }) => {
  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Записи</div>
      </PanelHeader>
      <Group header={<Header>Календарь и записи</Header>}>
        <Div>
          <Placeholder>Здесь будут отображаться записи клиентов и расписание.</Placeholder>
        </Div>
      </Group>
    </Panel>
  );
};

export default Appointments;
